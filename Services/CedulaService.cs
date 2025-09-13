// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// 
// Credits and inspiration: Acontplus-S-A-S team
// -------------------------------------------------------------

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Logging;
using Yamgooo.SRI.Client.Contracts;
using Yamgooo.SRI.Client.Models;

namespace Yamgooo.SRI.Client.Services
{
    public class CedulaService : ICedulaService
    {
        private readonly ILogger<CedulaService> _logger;
        private readonly HttpClient _httpClient;
        private readonly ICookieService _cookieService;
        private readonly ICaptchaService _captchaService;

        public CedulaService(
            ILogger<CedulaService> logger, 
            ICookieService cookieService,
            ICaptchaService captchaService,
            HttpClient? httpClient = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
            _captchaService = captchaService ?? throw new ArgumentNullException(nameof(captchaService));
            _httpClient = httpClient ?? new HttpClient();
            
            ConfigureHttpClient();
        }

        /// <summary>
        /// Retrieves contributor information from SRI using their cedula number
        /// </summary>
        public async Task<ContribuyenteCedulaDto> GetCedulaSriAsync(string cedula)
        {
            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("Starting cedula lookup. RequestId: {RequestId}, Cedula: {Cedula}", requestId, cedula);

            // Validate cedula input
            ValidateCedulaInput(cedula);

            // Check if cedula exists
            await CheckExistenceAsync(cedula, requestId);

            // Get cookies and captcha
            var cookieResult = await _cookieService.GetAsync();
            var captchaResult = await _captchaService.ValidateAsync(
                cookieResult.Captcha, 
                cookieResult.Cookie
            );

            var contributorData = await GetContributorDataAsync(
                cedula, 
                cookieResult.Cookie, 
                captchaResult, 
                requestId
            );

            _logger.LogInformation("Cedula lookup successful. RequestId: {RequestId}, Cedula: {Cedula}", 
                requestId, cedula);

            return contributorData;
        }

        #region Private Methods

        private void ConfigureHttpClient()
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0");
        }

        private void ValidateCedulaInput(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                throw new ArgumentException("Cédula es requerido", nameof(cedula));
            }

            if (cedula.Length != 10)
            {
                throw new ArgumentException("Cédula debe tener 10 dígitos", nameof(cedula));
            }

            if (!cedula.All(char.IsDigit))
            {
                throw new ArgumentException("Cédula debe contener solo números", nameof(cedula));
            }
        }

        private async Task CheckExistenceAsync(string cedula, string requestId)
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    Credentials = CredentialCache.DefaultNetworkCredentials
                };

                using var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0");

                var requestUri = new Uri(
                    $"https://srienlinea.sri.gob.ec/sri-registro-civil-servicio-internet/rest/DatosRegistroCivil/existeNumeroIdentificacion?numeroIdentificacion={cedula}"
                );

                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                _logger.LogDebug("Checking cedula existence. RequestId: {RequestId}, Cedula: {Cedula}", 
                    requestId, cedula);

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Cedula existence check failed. Status: {StatusCode}, RequestId: {RequestId}", 
                        response.StatusCode, requestId);
                    
                    throw new HttpRequestException("No se pudo consultar la existencia de la cédula");
                }

                var stream = await response.Content.ReadAsStreamAsync();
                using var streamReader = new StreamReader(stream);
                var html = HttpUtility.HtmlDecode(await streamReader.ReadToEndAsync());

                if (html == "true")
                {
                    _logger.LogDebug("Cedula exists. RequestId: {RequestId}, Cedula: {Cedula}", 
                        requestId, cedula);
                    return;
                }

                _logger.LogWarning("Cedula not found. RequestId: {RequestId}, Cedula: {Cedula}", 
                    requestId, cedula);
                throw new KeyNotFoundException("No existe contribuyente con esa cédula");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Request timeout during cedula existence check. RequestId: {RequestId}", requestId);
                throw new TimeoutException("Request timeout while checking cedula existence");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during cedula existence check. RequestId: {RequestId}", requestId);
                throw new InvalidOperationException($"Unexpected error during cedula existence check: {ex.Message}", ex);
            }
        }

        private async Task<ContribuyenteCedulaDto> GetContributorDataAsync(
            string cedula, 
            CookieContainer cookies, 
            string captcha, 
            string requestId)
        {
            try
            {
                var jsonSettings = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
                var captchaDeserialized = JsonSerializer.Deserialize<TokenSri>(captcha, jsonSettings);
                if (captchaDeserialized == null || string.IsNullOrWhiteSpace(captchaDeserialized.Mensaje))
                {
                    throw new ArgumentException("Invalid captcha token format", nameof(captcha));
                }

                var tokenSri = captchaDeserialized.Mensaje;

                var handler = new HttpClientHandler
                {
                    Credentials = CredentialCache.DefaultNetworkCredentials,
                    UseCookies = true,
                    CookieContainer = cookies
                };

                using var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", tokenSri);

                var requestUri = new Uri(
                    $"https://srienlinea.sri.gob.ec/sri-registro-civil-servicio-internet/rest/DatosRegistroCivil/obtenerDatosCompletosPorNumeroIdentificacionConToken?numeroIdentificacion={cedula}"
                );

                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                _logger.LogDebug("Requesting contributor data. RequestId: {RequestId}, Cedula: {Cedula}", 
                    requestId, cedula);

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Contributor data request failed. Status: {StatusCode}, RequestId: {RequestId}", 
                        response.StatusCode, requestId);
                    
                    throw new HttpRequestException($"Failed to retrieve contributor data. Status: {(int)response.StatusCode} {response.ReasonPhrase}");
                }

                var stream = await response.Content.ReadAsStreamAsync();
                using var streamReader = new StreamReader(stream);
                var sriResponse = HttpUtility.HtmlDecode(await streamReader.ReadToEndAsync());
                
                // Clean up response format
                sriResponse = sriResponse.Replace("[", "").Replace("]", "");

                var result = JsonSerializer.Deserialize<ContribuyenteCedulaDto>(sriResponse, jsonSettings);
                if (result != null)
                {
                    _logger.LogDebug("Contributor data retrieved successfully. RequestId: {RequestId}, Cedula: {Cedula}", 
                        requestId, cedula);
                    return result;
                }

                _logger.LogWarning("No contributor data found. RequestId: {RequestId}, Cedula: {Cedula}", 
                    requestId, cedula);
                throw new KeyNotFoundException("No existe contribuyente con esa cédula");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error during contributor data retrieval. RequestId: {RequestId}", requestId);
                throw new InvalidOperationException($"Failed to parse contributor data: {ex.Message}", ex);
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Request timeout during contributor data retrieval. RequestId: {RequestId}", requestId);
                throw new TimeoutException("Request timeout while retrieving contributor data");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during contributor data retrieval. RequestId: {RequestId}", requestId);
                throw new InvalidOperationException($"Unexpected error during contributor data retrieval: {ex.Message}", ex);
            }
        }

        #endregion
    }
}