// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

using Yamgooo.SRI.Client.Common;
using Yamgooo.SRI.Client.Models;

namespace Yamgooo.SRI.Client.Contracts
{
    public interface IRucService
    {
        /// <summary>
        /// Retrieves contributor information from SRI using their RUC number
        /// </summary>
        /// <param name="ruc">The RUC number to search for</param>
        /// <returns>
        /// An <see cref="ApiResult"/> containing the contributor data if successful,
        /// or an error message and status code if it fails.
        /// </returns>
        Task<ApiResult<ContribuyenteCompleteDto>> GetRucSriAsync(string ruc);
    }
}
