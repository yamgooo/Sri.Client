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
    public class RucService : IRucService
    {
        private readonly ILogger<RucService> _logger;
        private readonly HttpClient _httpClient;
        private readonly ICookieService _cookieService;
        private readonly ICaptchaService _captchaService;

        public RucService(
            ILogger<RucService> logger, 
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
        /// Retrieves contributor information from SRI using their RUC number
        /// </summary>
        public async Task<ContribuyenteCompleteDto> GetRucSriAsync(string ruc)
        {
            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("Starting RUC lookup. RequestId: {RequestId}, RUC: {Ruc}", requestId, ruc);

            var validationResult = ValidateRucInput(ruc);
            if (!validationResult)
            {
                throw new ArgumentException("RUC validation failed");
            }

            await CheckExistenceAsync(ruc, requestId);

            // Get cookies and captcha
            var cookieResult = await _cookieService.GetAsync();
            var captchaResult = await _captchaService.ValidateAsync(
                cookieResult.Captcha, 
                cookieResult.Cookie
            );

            var contributorData = await GetContributorDataAsync(
                ruc, 
                cookieResult.Cookie, 
                captchaResult, 
                requestId
            );

            _logger.LogInformation("RUC lookup successful. RequestId: {RequestId}, RUC: {Ruc}", 
                requestId, ruc);

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

        private bool ValidateRucInput(string ruc)
        {
            if (string.IsNullOrWhiteSpace(ruc))
            {
                throw new ArgumentException("RUC es requerido", nameof(ruc));
            }

            if (ruc.Length != 13)
            {
                throw new ArgumentException("RUC debe tener 13 dígitos", nameof(ruc));
            }

            if (!ruc.All(char.IsDigit))
            {
                throw new ArgumentException("RUC debe contener solo números", nameof(ruc));
            }

            return true;
        }

        private async Task CheckExistenceAsync(string ruc, string requestId)
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
                    $"https://srienlinea.sri.gob.ec/sri-catastro-sujeto-servicio-internet/rest/ConsolidadoContribuyente/existePorNumeroRuc?numeroRuc={ruc}"
                );

                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                _logger.LogDebug("Checking RUC existence. RequestId: {RequestId}, RUC: {Ruc}", 
                    requestId, ruc);

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("RUC existence check failed. Status: {StatusCode}, RequestId: {RequestId}", 
                        response.StatusCode, requestId);
                    
                    throw new HttpRequestException("No se pudo consultar la existencia del RUC");
                }

                var stream = await response.Content.ReadAsStreamAsync();
                using var streamReader = new StreamReader(stream);
                var html = HttpUtility.HtmlDecode(await streamReader.ReadToEndAsync());

                if (html == "true")
                {
                    _logger.LogDebug("RUC exists. RequestId: {RequestId}, RUC: {Ruc}", 
                        requestId, ruc);
                    return;
                }

                _logger.LogWarning("RUC not found. RequestId: {RequestId}, RUC: {Ruc}", 
                    requestId, ruc);
                throw new KeyNotFoundException("No existe contribuyente con ese RUC");
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
                _logger.LogWarning("Request timeout during RUC existence check. RequestId: {RequestId}", requestId);
                throw new TimeoutException("Request timeout while checking RUC existence");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during RUC existence check. RequestId: {RequestId}", requestId);
                throw new InvalidOperationException($"Unexpected error during RUC existence check: {ex.Message}", ex);
            }
        }

        private async Task<ContribuyenteCompleteDto> GetContributorDataAsync(
            string ruc, 
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
                    $"https://srienlinea.sri.gob.ec/sri-catastro-sujeto-servicio-internet/rest/ConsolidadoContribuyente/obtenerPorNumerosRuc?&ruc={ruc}"
                );

                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                _logger.LogDebug("Requesting contributor data. RequestId: {RequestId}, RUC: {Ruc}", 
                    requestId, ruc);

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

                var rucs = JsonSerializer.Deserialize<List<ContribuyenteRucDto>>(sriResponse, jsonSettings);
                if (rucs == null || rucs.Count == 0 || rucs[0].NumeroRuc != ruc)
                {
                    _logger.LogWarning("No contributor data found. RequestId: {RequestId}, RUC: {Ruc}", 
                        requestId, ruc);
                    throw new KeyNotFoundException("No existe contribuyente con ese RUC");
                }

                var consolidatedRuc = await GetRucWithEstablishmentsAsync(rucs[0], cookies, tokenSri, requestId);
                return consolidatedRuc;
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

        private async Task<ContribuyenteCompleteDto> GetRucWithEstablishmentsAsync(
            ContribuyenteRucDto contribuyenteRucDto,
            CookieContainer cookieContainer,
            string tokenSri,
            string requestId)
        {
            try
            {
                var jsonSettings = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                var handler = new HttpClientHandler
                {
                    Credentials = CredentialCache.DefaultNetworkCredentials,
                    UseCookies = true,
                    CookieContainer = cookieContainer
                };

                using var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", tokenSri);

                var requestUri = new Uri(
                    $"https://srienlinea.sri.gob.ec/sri-catastro-sujeto-servicio-internet/rest/Establecimiento/consultarPorNumeroRuc?numeroRuc={contribuyenteRucDto.NumeroRuc}"
                );

                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                _logger.LogDebug("Requesting establishments data. RequestId: {RequestId}, RUC: {Ruc}", 
                    requestId, contribuyenteRucDto.NumeroRuc);

                var response = await client.SendAsync(request);

                var establecimientos = new List<EstablecimientoDto>();

                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    using var streamReader = new StreamReader(stream);
                    var serializedEstabs = HttpUtility.HtmlDecode(await streamReader.ReadToEndAsync());
                    establecimientos = JsonSerializer.Deserialize<List<EstablecimientoDto>>(serializedEstabs, jsonSettings) ?? new List<EstablecimientoDto>();
                }
                else
                {
                    _logger.LogWarning("Establishments request failed. Status: {StatusCode}, RequestId: {RequestId}", 
                        response.StatusCode, requestId);
                }

                var result = new ContribuyenteCompleteDto
                {
                    Contribuyente = contribuyenteRucDto,
                    Establecimientos = establecimientos
                };

                // Set default address and commercial name from first establishment if available
                if (establecimientos.Count > 0)
                {
                    result.Contribuyente.Direccion = establecimientos[0].DireccionCompleta;
                    if (!string.IsNullOrWhiteSpace(establecimientos[0].NombreFantasiaComercial))
                    {
                        result.Contribuyente.NombreComercial = establecimientos[0].NombreFantasiaComercial;
                    }
                }

                _logger.LogDebug("Contributor data with establishments retrieved successfully. RequestId: {RequestId}, RUC: {Ruc}", 
                    requestId, contribuyenteRucDto.NumeroRuc);

                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error during establishments retrieval. RequestId: {RequestId}", requestId);
                throw new InvalidOperationException($"Failed to parse establishments data: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error during establishments retrieval. RequestId: {RequestId}", requestId);
                throw new HttpRequestException($"HTTP error during establishments retrieval: {ex.Message}", ex);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Request timeout during establishments retrieval. RequestId: {RequestId}", requestId);
                throw new TimeoutException("Request timeout while retrieving establishments data");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during establishments retrieval. RequestId: {RequestId}", requestId);
                throw new InvalidOperationException($"Unexpected error during establishments retrieval: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
