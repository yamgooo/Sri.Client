// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Yamgooo.SRI.Client.Models
{
    /// <summary>
    /// Información principal del contribuyente RUC
    /// </summary>
    public record ContribuyenteRucDto
    {
        [Required]
        public required string NumeroRuc { get; set; }

        [Required]
        public required string RazonSocial { get; set; }

        [Required]
        public required string EstadoContribuyenteRuc { get; set; }

        [Required]
        public required string ActividadEconomicaPrincipal { get; set; }

        [Required]
        public required string TipoContribuyente { get; set; }

        [Required]
        public required string Regimen { get; set; }

        [Required]
        public required string Categoria { get; set; }

        [Required]
        public required string ObligadoLlevarContabilidad { get; set; }

        [Required]
        public required string AgenteRetencion { get; set; }

        [Required]
        public required string ContribuyenteEspecial { get; set; }

        [Required]
        public required InformacionFechasContribuyenteDto InformacionFechasContribuyente { get; set; }

        public object[]? RepresentantesLegales { get; set; }

        public string? MotivoCancelacionSuspension { get; set; }

        [Required]
        public required string ContribuyenteFantasma { get; set; }

        [Required]
        public required string TransaccionesInexistente { get; set; }

        // Propiedades personalizadas
        public string? NombreComercial { get; set; }
        public string? Direccion { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
    }
}
