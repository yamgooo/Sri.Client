// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

namespace Yamgooo.SRI.Client.Models
{
    /// <summary>
    /// DTO for captcha image data
    /// </summary>
    public record CaptchaImageDto
    {
        public required string ImageName { get; set; }
        public required string ImageFieldName { get; set; }
        public required List<string> Values { get; set; }
        public required string AudioFieldName { get; set; }
    }
}
