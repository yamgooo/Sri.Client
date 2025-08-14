// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

using System.Net;
using Yamgooo.SRI.Client.Common;

namespace Yamgooo.SRI.Client.Contracts;

public interface ICookieService
{
    /// <summary>
    /// Retrieves a captcha image and its associated cookies from SRI.
    /// </summary>
    /// <returns>
    /// An <see cref="ApiResult"/> containing the captcha bytes and cookies if successful,
    /// or an error message and status code if it fails.
    /// </returns>
    Task<ApiResult<CookieResponse>> GetAsync();
}

public class CookieResponse
{
    /// <summary>
    /// Cookies used to maintain the SRI session.
    /// </summary>
    public required CookieContainer Cookie { get; set; }

    /// <summary>
    /// The captcha image in raw byte array format.
    /// </summary>
    public string Captcha { get; set; } = string.Empty;
}