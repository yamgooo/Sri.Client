// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

namespace Yamgooo.SRI.Client.Models
{
    /// <summary>
    /// Result for document validation/reception operations
    /// </summary>
    public class SriValidationResult : SriBaseResult
    {
        public string AccessKey { get; set; } = string.Empty;
    }
}
