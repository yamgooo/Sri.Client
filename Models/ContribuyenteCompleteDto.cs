// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Yamgooo.SRI.Client.Models
{
    /// <summary>
    /// DTO de respuesta combinada con contribuyente y establecimientos
    /// </summary>
    public record ContribuyenteCompleteDto
    {
        [Required]
        public required ContribuyenteRucDto Contribuyente { get; set; }

        [Required]
        public required List<EstablecimientoDto> Establecimientos { get; set; }
    }
}
