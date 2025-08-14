using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Yamgooo.SRI.Client.Contracts;
using Yamgooo.SRI.Client.Models;

namespace Yamgooo.SRI.Client.Services
{
    public class SriClientService : ISriClientService
    {
        private readonly ILogger<SriClientService> _logger;
        private readonly HttpClient _httpClient;
        private readonly SriServiceConfiguration _configuration;

        // SRI endpoints
        private const string TestValidationEndpoint = "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline";
        private const string TestAuthorizationEndpoint = "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline";
        private const string ProductionValidationEndpoint = "https://cel.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline";
        private const string ProductionAuthorizationEndpoint = "https://cel.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline";

        public SriClientService(ILogger<SriClientService> logger, HttpClient? httpClient = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? new HttpClient();
            _configuration = new SriServiceConfiguration();
            
            ConfigureHttpClient();
        }

        /// <summary>
        /// Validates and sends a signed XML document to SRI
        /// </summary>
        public async Task<SriValidationResult> ValidateDocumentAsync(string signedXml, SriEnvironment environment = SriEnvironment.Test)
        {
            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("Starting document validation. RequestId: {RequestId}, Environment: {Environment}", requestId, environment);

            try
            {
                if (string.IsNullOrWhiteSpace(signedXml))
                    return CreateValidationError("Signed XML cannot be null or empty", SriDocumentStatus.Returned, requestId);

                var accessKey = ExtractAccessKeyFromXml(signedXml);
                if (string.IsNullOrEmpty(accessKey))
                    return CreateValidationError("Could not extract access key from XML", SriDocumentStatus.Returned, requestId);

                var xmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(signedXml));

                var soapRequest = BuildValidationSoapRequest(xmlBase64);

                var endpoint = GetValidationEndpoint(environment);

                var response = await SendRequestWithRetryAsync(endpoint, soapRequest, requestId);

                return ParseValidationResponse(response, accessKey, requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during document validation. RequestId: {RequestId}", requestId);
                return CreateValidationError($"Unexpected error: {ex.Message}", SriDocumentStatus.Returned, requestId);
            }
        }

        /// <summary>
        /// Requests authorization for a document using its access key
        /// </summary>
        public async Task<SriAuthorizationResult> RequestAuthorizationAsync(string accessKey, SriEnvironment environment = SriEnvironment.Test)
        {
            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("Starting authorization request. RequestId: {RequestId}, AccessKey: {AccessKey}, Environment: {Environment}", 
                requestId, accessKey, environment);

            try
            {
                if (string.IsNullOrWhiteSpace(accessKey))
                    return CreateAuthorizationError("Access key cannot be null or empty", SriDocumentStatus.NotAuthorized, requestId);

                var soapRequest = BuildAuthorizationSoapRequest(accessKey);

                var endpoint = GetAuthorizationEndpoint(environment);

                var response = await SendRequestWithRetryAsync(endpoint, soapRequest, requestId);

                return ParseAuthorizationResponse(response, accessKey, requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during authorization request. RequestId: {RequestId}", requestId);
                return CreateAuthorizationError($"Unexpected error: {ex.Message}", SriDocumentStatus.NotAuthorized, requestId);
            }
        }

        /// <summary>
        /// Gets the current SRI service configuration
        /// </summary>
        public SriServiceConfiguration GetConfiguration()
        {
            return _configuration;
        }

        /// <summary>
        /// Updates the SRI service configuration
        /// </summary>
        public void UpdateConfiguration(SriServiceConfiguration configuration)
        {
             _configuration.TimeoutSeconds = configuration.TimeoutSeconds;
            _configuration.MaxRetries = configuration.MaxRetries;
            _configuration.RetryDelaySeconds = configuration.RetryDelaySeconds;
            
            ConfigureHttpClient();
        }

        #region Private Methods

        private void ConfigureHttpClient()
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(_configuration.TimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("SOAPAction", "");
        }

        private string GetValidationEndpoint(SriEnvironment environment)
        {
            return environment == SriEnvironment.Test ? TestValidationEndpoint : ProductionValidationEndpoint;
        }

        private string GetAuthorizationEndpoint(SriEnvironment environment)
        {
            return environment == SriEnvironment.Test ? TestAuthorizationEndpoint : ProductionAuthorizationEndpoint;
        }

        private string ExtractAccessKeyFromXml(string xml)
        {
            try
            {
                var doc = XDocument.Parse(xml);
                var accessKeyElement = doc.Descendants("claveAcceso").FirstOrDefault();
                return accessKeyElement?.Value ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract access key from XML");
                return string.Empty;
            }
        }

        private string BuildValidationSoapRequest(string xmlBase64)
        {
            return $"""
                <soapenv:Envelope 
                    xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" 
                    xmlns:ec="http://ec.gob.sri.ws.recepcion">
                    <soapenv:Header/>
                    <soapenv:Body>
                        <ec:validarComprobante>
                            <xml>{xmlBase64}</xml>
                        </ec:validarComprobante>
                    </soapenv:Body>
                </soapenv:Envelope>
                """;
        }

        private string BuildAuthorizationSoapRequest(string accessKey)
        {
            return $"""
                <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                    <soap:Header/>
                    <soap:Body>
                        <ns2:autorizacionComprobante xmlns:ns2="http://ec.gob.sri.ws.autorizacion">
                            <claveAccesoComprobante>{accessKey}</claveAccesoComprobante>
                        </ns2:autorizacionComprobante>
                    </soap:Body>
                </soap:Envelope>
                """;
        }

        private async Task<string> SendRequestWithRetryAsync(string endpoint, string soapRequest, string requestId)
        {
            for (var attempt = 1; attempt <= _configuration.MaxRetries; attempt++)
            {
                try
                {
                    _logger.LogDebug("Sending request attempt {Attempt}/{MaxRetries}. RequestId: {RequestId}", 
                        attempt, _configuration.MaxRetries, requestId);

                    var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
                    var response = await _httpClient.PostAsync(endpoint, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation("Request completed. Status: {StatusCode}, RequestId: {RequestId}", 
                        response.StatusCode, requestId);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
                    }

                    return responseContent;
                }
                catch (TaskCanceledException ex) when (attempt < _configuration.MaxRetries)
                {
                    _logger.LogWarning(ex, "Request timeout on attempt {Attempt}/{MaxRetries}. RequestId: {RequestId}", 
                        attempt, _configuration.MaxRetries, requestId);

                    if (attempt >= _configuration.MaxRetries) throw;
                    await Task.Delay(TimeSpan.FromSeconds(_configuration.RetryDelaySeconds));
                }
                catch (HttpRequestException ex) when (attempt < _configuration.MaxRetries)
                {
                    _logger.LogWarning(ex, "HTTP error on attempt {Attempt}/{MaxRetries}. RequestId: {RequestId}", 
                        attempt, _configuration.MaxRetries, requestId);

                    if (attempt >= _configuration.MaxRetries) throw;
                    await Task.Delay(TimeSpan.FromSeconds(_configuration.RetryDelaySeconds));
                }
            }

            throw new InvalidOperationException($"All {_configuration.MaxRetries} attempts failed");
        }

        private SriValidationResult ParseValidationResponse(string responseContent, string accessKey, string requestId)
        {
            try
            {
                var xdoc = XDocument.Parse(responseContent);
                
                var estado = xdoc.Descendants("estado").FirstOrDefault()?.Value ?? "DEVUELTA";
                
                var status = estado == "RECIBIDA" ? SriDocumentStatus.Received : SriDocumentStatus.Returned;
                var success = status == SriDocumentStatus.Received;

                var result = new SriValidationResult
                {
                    Success = success,
                    Status = status,
                    AccessKey = accessKey,
                    RequestId = requestId,
                    ProcessedAt = DateTime.UtcNow
                };

                var mensajes = xdoc.Descendants("mensaje").Select(m => new SriMessage
                {
                    Identifier = m.Element("identificador")?.Value ?? string.Empty,
                    Message = m.Element("mensaje")?.Value ?? string.Empty,
                    AdditionalInformation = m.Element("informacionAdicional")?.Value ?? string.Empty,
                    Type = ParseMessageType(m.Element("tipo")?.Value ?? "ERROR")
                }).ToList();

                result.Messages = mensajes.Where(m => !string.IsNullOrEmpty(m.Message)).ToList();

                switch (success)
                {
                    case false when mensajes.Any():
                        result.ErrorMessage = string.Join("; ", mensajes.Select(m => $"{m.Message}: {m.AdditionalInformation}"));
                        _logger.LogWarning("Document validation failed. RequestId: {RequestId}, Errors: {Errors}", 
                            requestId, result.ErrorMessage);
                        break;
                    case true:
                        _logger.LogInformation("Document validation successful. RequestId: {RequestId}", requestId);
                        break;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse validation response. RequestId: {RequestId}", requestId);
                return CreateValidationError($"Failed to parse response: {ex.Message}", SriDocumentStatus.Returned, requestId);
            }
        }

        private SriAuthorizationResult ParseAuthorizationResponse(string responseContent, string accessKey, string requestId)
        {
            try
            {
                var xdoc = XDocument.Parse(responseContent);
                
                var respNode = xdoc.Descendants()
                    .FirstOrDefault(n => n.Name.LocalName == "RespuestaAutorizacionComprobante");

                if (respNode == null)
                {
                    return CreateAuthorizationError("Invalid response format - missing RespuestaAutorizacionComprobante", 
                        SriDocumentStatus.NotAuthorized, requestId);
                }

                var autorizaciones = respNode.Descendants()
                    .Where(n => n.Name.LocalName == "autorizacion")
                    .Select(a => new
                    {
                        Estado = a.Descendants().FirstOrDefault(x => x.Name.LocalName == "estado")?.Value ?? "NO AUTORIZADO",
                        NumeroAutorizacion = a.Descendants().FirstOrDefault(x => x.Name.LocalName == "numeroAutorizacion")?.Value ?? "",
                        FechaAutorizacion = ParseDateTime(a.Descendants().FirstOrDefault(x => x.Name.LocalName == "fechaAutorizacion")?.Value),
                        Ambiente = a.Descendants().FirstOrDefault(x => x.Name.LocalName == "ambiente")?.Value ?? "",
                        Comprobante = a.Descendants().FirstOrDefault(x => x.Name.LocalName == "comprobante")?.Value ?? ""
                    })
                    .ToList();

                if (!autorizaciones.Any())
                {
                    return CreateAuthorizationError("No authorization data found in response", 
                        SriDocumentStatus.NotAuthorized, requestId);
                }

                var autorizacion = autorizaciones.First();
                var status = autorizacion.Estado == "AUTORIZADO" ? SriDocumentStatus.Authorized : SriDocumentStatus.NotAuthorized;
                var success = status == SriDocumentStatus.Authorized;

                var result = new SriAuthorizationResult
                {
                    Success = success,
                    Status = status,
                    AccessKey = accessKey,
                    AuthorizationNumber = autorizacion.NumeroAutorizacion,
                    AuthorizationDate = autorizacion.FechaAutorizacion,
                    Environment = autorizacion.Ambiente,
                    DocumentContent = autorizacion.Comprobante,
                    DocumentCount = autorizaciones.Count,
                    RequestId = requestId,
                    ProcessedAt = DateTime.UtcNow
                };

                var mensajes = respNode.Descendants("mensaje")
                    .Where(m =>  m.Element("identificador")?.Value is not null)
                    .Select(m => new SriMessage
                {
                    Identifier = m.Element("identificador")?.Value ?? string.Empty,
                    Message = m.Element("mensaje")?.Value ?? string.Empty,
                    AdditionalInformation = m.Element("informacionAdicional")?.Value ?? string.Empty,
                    Type = ParseMessageType(m.Element("tipo")?.Value ?? "ERROR")
                }).ToList();

                result.Messages = mensajes;

                switch (success)
                {
                    case false when mensajes.Any():
                        result.ErrorMessage = string.Join("; ", mensajes.Select(m => $"{m.Message}: {m.AdditionalInformation}"));
                        _logger.LogWarning("Document authorization failed. RequestId: {RequestId}, Errors: {Errors}", 
                            requestId, result.ErrorMessage);
                        break;
                    case true:
                        _logger.LogInformation("Document authorization successful. RequestId: {RequestId}, AuthNumber: {AuthNumber}", 
                            requestId, autorizacion.NumeroAutorizacion);
                        break;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse authorization response. RequestId: {RequestId}", requestId);
                return CreateAuthorizationError($"Failed to parse response: {ex.Message}", SriDocumentStatus.NotAuthorized, requestId);
            }
        }

        private SriMessageType ParseMessageType(string type)
        {
            return type.ToUpperInvariant() switch
            {
                "ERROR" => SriMessageType.Error,
                "WARNING" => SriMessageType.Warning,
                "INFO" => SriMessageType.Info,
                _ => SriMessageType.Error
            };
        }

        private static DateTime? ParseDateTime(string? dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return null;

            return DateTime.TryParse(dateString, out var result) ? result : null;
        }

        private SriValidationResult CreateValidationError(string errorMessage, SriDocumentStatus status, string requestId)
        {
            return new SriValidationResult
            {
                Success = false,
                Status = status,
                ErrorMessage = errorMessage,
                RequestId = requestId,
                ProcessedAt = DateTime.UtcNow,
                Messages =
                [
                    new SriMessage
                    {
                        Message = errorMessage,
                        Type = SriMessageType.Error
                    }
                ]
            };
        }

        private SriAuthorizationResult CreateAuthorizationError(string errorMessage, SriDocumentStatus status, string requestId)
        {
            return new SriAuthorizationResult
            {
                Success = false,
                Status = status,
                ErrorMessage = errorMessage,
                RequestId = requestId,
                ProcessedAt = DateTime.UtcNow,
                Messages =
                [
                    new SriMessage
                    {
                        Message = errorMessage,
                        Type = SriMessageType.Error
                    }
                ]
            };
        }

        #endregion
    }
} 