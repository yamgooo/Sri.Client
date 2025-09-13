// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-09-01
// -------------------------------------------------------------

using System.Net;

namespace Yamgooo.SRI.Client.Models;

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