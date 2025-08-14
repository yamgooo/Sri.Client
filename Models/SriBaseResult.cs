// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

namespace Yamgooo.SRI.Client.Models
{
    /// <summary>
    /// Base result for SRI operations
    /// </summary>
    public abstract class SriBaseResult
    {
        public bool Success { get; set; }
        public SriDocumentStatus Status { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<SriMessage> Messages { get; set; } = new();
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
    }
}
