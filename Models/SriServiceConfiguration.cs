// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-08-13
// -------------------------------------------------------------

namespace Yamgooo.SRI.Client.Models
{
    /// <summary>
    /// Configuration for SRI service
    /// </summary>
    public class SriServiceConfiguration
    {
        public int TimeoutSeconds { get; set; } = 120;
        public int MaxRetries { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 5;

        /// <summary>
        /// Validates the configuration
        /// </summary>
        /// <returns>True if configuration is valid</returns>
        public bool IsValid()
        {
            return TimeoutSeconds > 0 && 
                   MaxRetries >= 0 && 
                   RetryDelaySeconds >= 0;
        }

        /// <summary>
        /// Gets validation error message if configuration is invalid
        /// </summary>
        /// <returns>Validation error message</returns>
        public string GetValidationError()
        {
            if (TimeoutSeconds <= 0)
                return "TimeoutSeconds must be greater than 0";
            
            if (MaxRetries < 0)
                return "MaxRetries must be 0 or greater";
            
            if (RetryDelaySeconds < 0)
                return "RetryDelaySeconds must be 0 or greater";
            
            return string.Empty;
        }
    }
}
