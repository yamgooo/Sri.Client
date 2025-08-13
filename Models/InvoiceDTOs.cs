
namespace Yamgooo.SRI.Client.Models
{
    /// <summary>
    /// Environment types for SRI services
    /// </summary>
    public enum SriEnvironment
    {
        Test = 1,
        Production = 2
    }

    /// <summary>
    /// SRI document status
    /// </summary>
    public enum SriDocumentStatus
    {
        Received = 1,
        Returned = 2,
        Authorized = 3,
        NotAuthorized = 4
    }

    /// <summary>
    /// SRI message types
    /// </summary>
    public enum SriMessageType
    {
        Error = 1,
        Warning = 2,
        Info = 3
    }

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

    /// <summary>
    /// Result for document validation/reception operations
    /// </summary>
    public class SriValidationResult : SriBaseResult
    {
        public string AccessKey { get; set; } = string.Empty;
    }

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