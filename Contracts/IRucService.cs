// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

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
        /// The contributor data if successful, or throws an exception if it fails.
        /// </returns>
        Task<ContribuyenteCompleteDto> GetRucSriAsync(string ruc);
    }
}
