// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

using System.Net;

namespace Yamgooo.SRI.Client.Contracts
{
    public interface ICaptchaService
    {
        /// <summary>
        /// Validates a captcha with SRI using the provided captcha data and cookies
        /// </summary>
        /// <param name="captcha">The captcha data in JSON format</param>
        /// <param name="cookies">The cookie container for the SRI session</param>
        /// <returns>
        /// The validation token if successful, or throws an exception if it fails.
        /// </returns>
        Task<string> ValidateAsync(string captcha, CookieContainer cookies);
    }
}
