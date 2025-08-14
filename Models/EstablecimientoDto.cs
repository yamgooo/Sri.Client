// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Yamgooo.SRI.Client.Models
{
    /// <summary>
    /// Información del establecimiento
    /// </summary>
    public record EstablecimientoDto
    {
        public string? NombreFantasiaComercial { get; set; }

        [Required]
        public required string TipoEstablecimiento { get; set; }

        [Required]
        public required string DireccionCompleta { get; set; }

        [Required]
        public required string Estado { get; set; }

        [Required]
        public required string NumeroEstablecimiento { get; set; }

        [Required]
        public required string Matriz { get; set; }
    }
}
