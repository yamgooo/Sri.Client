// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Yamgooo.SRI.Client.Models
{
    /// <summary>
    /// Información de fechas del contribuyente
    /// </summary>
    public record InformacionFechasContribuyenteDto
    {
        [Required]
        public required string FechaInicioActividades { get; set; }
        
        [Required]
        public required string FechaCese { get; set; }
        
        [Required]
        public required string FechaReinicioActividades { get; set; }
        
        [Required]
        public required string FechaActualizacion { get; set; }
    }
}
