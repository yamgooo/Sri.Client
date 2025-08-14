// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

namespace Yamgooo.SRI.Client.Models
{
    /// <summary>
    /// SRI message information
    /// </summary>
    public class SriMessage
    {
        public string Identifier { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string AdditionalInformation { get; set; } = string.Empty;
        public SriMessageType Type { get; set; }
    }
}
