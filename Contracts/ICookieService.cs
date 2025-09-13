// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

using System.Net;
using Yamgooo.SRI.Client.Models;

namespace Yamgooo.SRI.Client.Contracts;

public interface ICookieService
{
    /// <summary>
    /// Retrieves a captcha image and its associated cookies from SRI.
    /// </summary>
    /// <returns>
    /// The captcha bytes and cookies if successful, or throws an exception if it fails.
    /// </returns>
    Task<CookieResponse> GetAsync();
}
