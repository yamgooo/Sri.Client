// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

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
        /// The contributor data if successful, or throws an exception if it fails.
        /// </returns>
        Task<ContribuyenteCedulaDto> GetCedulaSriAsync(string cedula);
    }
}
