using Yamgooo.SRI.Client.Models;

namespace Yamgooo.SRI.Client
{
    /// <summary>
    /// Service interface for SRI (Servicio de Rentas Internas) operations
    /// </summary>
    public interface ISriClientService
    {
        /// <summary>
        /// Validates and sends a signed XML document to SRI
        /// </summary>
        /// <param name="signedXml">The signed XML document to validate</param>
        /// <param name="environment">The SRI environment to use</param>
        /// <returns>Validation result from SRI</returns>
        Task<SriValidationResult> ValidateDocumentAsync(string signedXml, SriEnvironment environment = SriEnvironment.Test);

        /// <summary>
        /// Requests authorization for a document using its access key
        /// </summary>
        /// <param name="accessKey">The document access key</param>
        /// <param name="environment">The SRI environment to use</param>
        /// <returns>Authorization result from SRI</returns>
        Task<SriAuthorizationResult> RequestAuthorizationAsync(string accessKey, SriEnvironment environment = SriEnvironment.Test);

        /// <summary>
        /// Gets the current SRI service configuration
        /// </summary>
        /// <returns>Current configuration</returns>
        SriServiceConfiguration GetConfiguration();

        /// <summary>
        /// Updates the SRI service configuration
        /// </summary>
        /// <param name="configuration">New configuration</param>
        void UpdateConfiguration(SriServiceConfiguration configuration);
    }
} 