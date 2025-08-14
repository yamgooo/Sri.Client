// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

using Yamgooo.SRI.Client.Common;
using Yamgooo.SRI.Client.Models;

namespace Yamgooo.SRI.Client.Contracts
{
    public interface ICedulaService
    {
        /// <summary>
        /// Retrieves contributor information from SRI using their cedula number
        /// </summary>
        /// <param name="cedula">The cedula number to search for</param>
        /// <returns>
        /// An <see cref="ApiResult"/> containing the contributor data if successful,
        /// or an error message and status code if it fails.
        /// </returns>
        Task<ApiResult<ContribuyenteCedulaDto>> GetCedulaSriAsync(string cedula);
    }
}
