// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

namespace Yamgooo.SRI.Client.Models
{
    /// <summary>
    /// Result for document authorization operations
    /// </summary>
    public class SriAuthorizationResult : SriBaseResult
    {
        public string AccessKey { get; set; } = string.Empty;
        public string AuthorizationNumber { get; set; } = string.Empty;
        public DateTime? AuthorizationDate { get; set; }
        public string Environment { get; set; } = string.Empty;
        public string DocumentContent { get; set; } = string.Empty;
        public int DocumentCount { get; set; }
    }
}
