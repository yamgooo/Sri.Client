// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

using System.Net;
using Yamgooo.SRI.Client.Common;

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
        /// An <see cref="ApiResult"/> containing the validation token if successful,
        /// or an error message and status code if it fails.
        /// </returns>
        Task<ApiResult<string>> ValidateAsync(string captcha, CookieContainer cookies);
    }
}
