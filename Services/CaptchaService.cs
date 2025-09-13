// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

using System.Net;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Logging;
using Yamgooo.SRI.Client.Contracts;
using Yamgooo.SRI.Client.Models;

namespace Yamgooo.SRI.Client.Services
{
    public class CaptchaService : ICaptchaService
    {
        private readonly ILogger<CaptchaService> _logger;
        private readonly HttpClient _httpClient;

        public CaptchaService(ILogger<CaptchaService> logger, HttpClient? httpClient = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? new HttpClient();
            
            ConfigureHttpClient();
        }

        /// <summary>
        /// Validates a captcha with SRI using the provided captcha data and cookies
        /// </summary>
        public async Task<string> ValidateAsync(string captcha, CookieContainer cookies)
        {
            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("Starting captcha validation. RequestId: {RequestId}", requestId);

            if (string.IsNullOrWhiteSpace(captcha))
            {
                throw new ArgumentException("Captcha data cannot be null or empty", nameof(captcha));
            }
            
            var jsonSettings = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var captchaImage = JsonSerializer.Deserialize<CaptchaImageDto>(captcha, jsonSettings);
            if (captchaImage?.Values == null || !captchaImage.Values.Any())
            {
                throw new ArgumentException("Invalid captcha data format", nameof(captcha));
            }

            var captchaImageValue = captchaImage.Values[0];
            if (string.IsNullOrWhiteSpace(captchaImageValue))
            {
                throw new ArgumentException("Captcha image value is empty", nameof(captcha));
            }

            var token = await ValidateCaptchaWithSriAsync(captchaImageValue, cookies, requestId);
            
            _logger.LogInformation("Captcha validation successful. RequestId: {RequestId}", requestId);
            return token;
        }

        #region Private Methods

        private void ConfigureHttpClient()
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0");
        }

        private async Task<string> ValidateCaptchaWithSriAsync(string captchaImageValue, CookieContainer cookies, string requestId)
        {
            try
            {
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

                var requestUri = new Uri(
                    $"https://srienlinea.sri.gob.ec/sri-captcha-servicio-internet/rest/ValidacionCaptcha/validarCaptcha/{captchaImageValue}?emitirToken=true"
                );

                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                _logger.LogDebug("Sending captcha validation request. RequestId: {RequestId}, CaptchaValue: {CaptchaValue}", 
                    requestId, captchaImageValue);

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Captcha validation failed. Status: {StatusCode}, RequestId: {RequestId}", 
                        response.StatusCode, requestId);
                    
                    throw new HttpRequestException($"No se pudo validar el captcha. Status: {(int)response.StatusCode} {response.ReasonPhrase}");
                }

                var stream = await response.Content.ReadAsStreamAsync();
                using var streamReader = new StreamReader(stream);
                var responseContent = HttpUtility.HtmlDecode(await streamReader.ReadToEndAsync());

                _logger.LogDebug("Captcha validation response received. RequestId: {RequestId}, ResponseLength: {ResponseLength}", 
                    requestId, responseContent.Length);

                return responseContent;
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Request timeout during captcha validation. RequestId: {RequestId}", requestId);
                throw new TimeoutException("Request timeout while validating captcha");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during captcha validation. RequestId: {RequestId}", requestId);
                throw new InvalidOperationException($"Unexpected error during captcha validation: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
