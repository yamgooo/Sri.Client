// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

namespace Yamgooo.SRI.Client.Models
{
    /// <summary>
    /// DTO for contributor data from SRI
    /// </summary>
    public class ContribuyenteCedulaDto
    {
        public string Identificacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string FechaDefuncion { get; set; } = string.Empty;
    }
}
