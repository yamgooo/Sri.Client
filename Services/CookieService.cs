// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

using System.Net;
using System.Security.Cryptography;
using System.Web;
using Yamgooo.SRI.Client.Common;
using Yamgooo.SRI.Client.Contracts;

namespace Yamgooo.SRI.Client.Services;

public class CookieService(IHttpClientFactory httpClientFactory) : ICookieService
{
    public async Task<ApiResult<CookieResponse>> GetAsync()
    {
        try
        {
            var cookieContainer = new CookieContainer();

            // Generate secure random number
            var generatedNumber = RandomNumberGenerator.GetInt32(0, 100_000_000).ToString("D6");

            var handler = new HttpClientHandler
            {
                Credentials = CredentialCache.DefaultNetworkCredentials,
                UseCookies = true,
                CookieContainer = cookieContainer
            };

            using var client = new HttpClient(handler);
            
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("User-Agent", 
                "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0");

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://srienlinea.sri.gob.ec/sri-captcha-servicio-internet/captcha/start/1?r={generatedNumber}"
            );

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResult<CookieResponse>(
                    false,
                    ApiResultStatusCode.BadRequest,
                    null,
                    $"Failed to obtain captcha. Status: {(int)response.StatusCode} {response.ReasonPhrase}"
                );
            }

            var stream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            var captcha = HttpUtility.HtmlDecode(await streamReader.ReadToEndAsync());

            return new ApiResult<CookieResponse>(
                true,
                ApiResultStatusCode.Success,
                new CookieResponse
                {
                    Cookie = cookieContainer,
                    Captcha = captcha
                }
            );
        }
        catch (HttpRequestException ex)
        {
            return new ApiResult<CookieResponse>(
                false,
                ApiResultStatusCode.ServerError,
                null,
                $"HTTP error: {ex.Message}"
            );
        }
        catch (TaskCanceledException)
        {
            return new ApiResult<CookieResponse>(
                false,
                ApiResultStatusCode.ServerError,
                null,
                "Request timeout while retrieving captcha"
            );
        }
        catch (Exception ex)
        {
            return new ApiResult<CookieResponse>(
                false,
                ApiResultStatusCode.ServerError,
                null,
                $"Unexpected error: {ex.Message}"
            );
        }
    }
}
