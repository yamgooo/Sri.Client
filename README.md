# SRI Client Service

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![NuGet](https://img.shields.io/badge/nuget-Yamgooo.SRI.Client-blue.svg)](https://www.nuget.org/packages/Yamgooo.SRI.Client)

A professional .NET library for SRI (Servicio de Rentas Internas) client operations in Ecuador. This package provides seamless integration with SRI web services for document validation and authorization of electronic invoices, as well as contributor information queries.

Also available in Spanish: [README_es.md](README_es.md)

## 🚀 Features

- **Document Validation**: Send signed XML documents to SRI for validation and reception
- **Authorization Requests**: Request document authorization using access keys
- **Contributor Queries**: Get complete contributor information by RUC or cedula
- **Existence Validation**: Automatically verify if contributors exist before retrieving their data
- **Environment Support**: Support for both test and production SRI environments
- **Async Operations**: High-performance asynchronous operations
- **Configuration Support**: Multiple configuration options (appsettings.json, code-based, dynamic)
- **Retry Logic**: Built-in retry mechanism with configurable parameters
- **Logging**: Comprehensive logging with structured logging support
- **Error Handling**: Robust error handling with detailed error messages
- **Performance Monitoring**: Built-in performance metrics and timing

## 📦 Installation

### NuGet Package
```bash
dotnet add package Yamgooo.SRI.Client
```

### Manual Installation
```bash
git clone https://github.com/yamgooo/Sri.Client.git
cd Sri.Client
dotnet build
```

## 🛠️ Quick Start

### 1. Basic Usage (Default Configuration)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Yamgooo.SRI.Client;

var services = new ServiceCollection();

// Register the service with default configuration
services.AddLogging();
services.AddSriClientService();

var provider = services.BuildServiceProvider();
var sriClient = provider.GetRequiredService<ISriClientService>();

// Validate a signed document
var validationResult = await sriClient.ValidateDocumentAsync(signedXml, SriEnvironment.Test);

if (validationResult.Success)
{
    Console.WriteLine($"Document validated successfully. Access Key: {validationResult.AccessKey}");
    
    // Request authorization
    var authResult = await sriClient.RequestAuthorizationAsync(validationResult.AccessKey, SriEnvironment.Test);
    
    if (authResult.Success)
    {
        Console.WriteLine($"Document authorized. Number: {authResult.AuthorizationNumber}");
    }
}
```

### 2. Configuration-based Usage

#### appsettings.json
```json
{
  "SriClient": {
    "TimeoutSeconds": 120,
    "MaxRetries": 3,
    "RetryDelaySeconds": 5
  }
}
```

#### Program.cs
```csharp
// Register with configuration
services.AddSriClientService(configuration);

// Use the service
var sriClient = serviceProvider.GetRequiredService<ISriClientService>();
var result = await sriClient.ValidateDocumentAsync(signedXml);
```

### 3. Custom Configuration

```csharp
var config = new SriServiceConfiguration
{
    TimeoutSeconds = 180,
    MaxRetries = 5,
    RetryDelaySeconds = 10
};

services.AddSriClientService(config);
```

## 📋 API Reference

### ISriClientService Interface

#### Document Validation
```csharp
Task<SriValidationResult> ValidateDocumentAsync(string signedXml, SriEnvironment environment = SriEnvironment.Test);
```

#### Authorization Request
```csharp
Task<SriAuthorizationResult> RequestAuthorizationAsync(string accessKey, SriEnvironment environment = SriEnvironment.Test);
```

#### Configuration Management
```csharp
SriServiceConfiguration GetConfiguration();
void UpdateConfiguration(SriServiceConfiguration configuration);
```

### IRucService Interface

#### Contributor Query by RUC
```csharp
Task<ApiResult<ContribuyenteCompleteDto>> GetRucSriAsync(string ruc);
```

**Features:**
- Automatically validates that RUC has 13 numeric digits
- Verifies contributor existence in SRI before retrieving data
- Returns complete contributor information including establishments
- Robust error handling with specific status codes

### ICedulaService Interface

#### Contributor Query by Cedula
```csharp
Task<ApiResult<ContribuyenteCedulaDto>> GetCedulaSriAsync(string cedula);
```

**Features:**
- Automatically validates that cedula has 10 numeric digits
- Verifies contributor existence in Civil Registry before retrieving data
- Returns basic contributor information (identification, full name, death date)
- Robust error handling with specific status codes

### Result Models

#### SriValidationResult
```csharp
public class SriValidationResult : SriBaseResult
{
    public string AccessKey { get; set; }
}
```

#### SriAuthorizationResult
```csharp
public class SriAuthorizationResult : SriBaseResult
{
    public string AccessKey { get; set; }
    public string AuthorizationNumber { get; set; }
    public DateTime? AuthorizationDate { get; set; }
    public string Environment { get; set; }
    public string DocumentContent { get; set; }
    public int DocumentCount { get; set; }
}
```

#### ContribuyenteCompleteDto
```csharp
public record ContribuyenteCompleteDto
{
    public required ContribuyenteRucDto Contribuyente { get; set; }
    public required List<EstablecimientoDto> Establecimientos { get; set; }
}
```

#### ContribuyenteCedulaDto
```csharp
public class ContribuyenteCedulaDto
{
    public string Identificacion { get; set; }
    public string NombreCompleto { get; set; }
    public string FechaDefuncion { get; set; }
}
```

## 🔧 Configuration Options

### Service Registration Methods

```csharp
// From appsettings.json
services.AddSriClientService(configuration, "SriClient");

// With custom configuration object
services.AddSriClientService(customConfig);

// With direct parameters
services.AddSriClientService(timeoutSeconds: 120, maxRetries: 3, retryDelaySeconds: 5);

// Without configuration (uses defaults)
services.AddSriClientService();
```

### Configuration Properties

- **TimeoutSeconds**: Request timeout in seconds (default: 120)
- **MaxRetries**: Maximum number of retries (default: 3)
- **RetryDelaySeconds**: Delay between retries in seconds (default: 5)

## 📝 Examples

### Complete SRI Document Processing Example

```csharp
public class SriDocumentProcessor
{
    private readonly ISriClientService _sriClient;
    private readonly ILogger<SriDocumentProcessor> _logger;

    public SriDocumentProcessor(ISriClientService sriClient, ILogger<SriDocumentProcessor> logger)
    {
        _sriClient = sriClient;
        _logger = logger;
    }

    public async Task<ProcessingResult> ProcessDocumentAsync(string signedXml, SriEnvironment environment)
    {
        try
        {
            // Step 1: Validate document
            _logger.LogInformation("Starting document validation");
            var validationResult = await _sriClient.ValidateDocumentAsync(signedXml, environment);
            
            if (!validationResult.Success)
            {
                _logger.LogError("Document validation failed: {ErrorMessage}", validationResult.ErrorMessage);
                return ProcessingResult.CreateFailure(validationResult.ErrorMessage);
            }

            _logger.LogInformation("Document validated successfully. Access Key: {AccessKey}", validationResult.AccessKey);

            // Step 2: Request authorization
            _logger.LogInformation("Requesting document authorization");
            var authResult = await _sriClient.RequestAuthorizationAsync(validationResult.AccessKey, environment);
            
            if (!authResult.Success)
            {
                _logger.LogError("Document authorization failed: {ErrorMessage}", authResult.ErrorMessage);
                return ProcessingResult.CreateFailure(authResult.ErrorMessage);
            }

            _logger.LogInformation("Document authorized successfully. Number: {AuthNumber}", authResult.AuthorizationNumber);
            
            return ProcessingResult.CreateSuccess(authResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SRI document");
            return ProcessingResult.CreateFailure(ex.Message);
        }
    }
}
```

### Contributor Query by RUC Example

```csharp
public class ContributorService
{
    private readonly IRucService _rucService;
    private readonly ILogger<ContributorService> _logger;

    public ContributorService(IRucService rucService, ILogger<ContributorService> logger)
    {
        _rucService = rucService;
        _logger = logger;
    }

    public async Task<ApiResult<ContribuyenteCompleteDto>> QueryContributorByRucAsync(string ruc)
    {
        try
        {
            _logger.LogInformation("Querying contributor by RUC: {Ruc}", ruc);
            
            var result = await _rucService.GetRucSriAsync(ruc);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Contributor found: {Name}", result.Data.Contribuyente.RazonSocial);
                _logger.LogInformation("Establishments found: {Count}", result.Data.Establecimientos.Count);
            }
            else
            {
                _logger.LogWarning("No contributor found with RUC: {Ruc}. Error: {Error}", 
                    ruc, result.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying contributor by RUC: {Ruc}", ruc);
            return new ApiResult<ContribuyenteCompleteDto>(
                false,
                ApiResultStatusCode.ServerError,
                null,
                $"Unexpected error: {ex.Message}"
            );
        }
    }
}
```

### Contributor Query by Cedula Example

```csharp
public class CedulaService
{
    private readonly ICedulaService _cedulaService;
    private readonly ILogger<CedulaService> _logger;

    public CedulaService(ICedulaService cedulaService, ILogger<CedulaService> logger)
    {
        _cedulaService = cedulaService;
        _logger = logger;
    }

    public async Task<ApiResult<ContribuyenteCedulaDto>> QueryContributorByCedulaAsync(string cedula)
    {
        try
        {
            _logger.LogInformation("Querying contributor by cedula: {Cedula}", cedula);
            
            var result = await _cedulaService.GetCedulaSriAsync(cedula);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Contributor found: {Name}", result.Data.NombreCompleto);
                
                if (!string.IsNullOrEmpty(result.Data.FechaDefuncion))
                {
                    _logger.LogWarning("Contributor deceased: {DeathDate}", result.Data.FechaDefuncion);
                }
            }
            else
            {
                _logger.LogWarning("No contributor found with cedula: {Cedula}. Error: {Error}", 
                    cedula, result.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying contributor by cedula: {Cedula}", cedula);
            return new ApiResult<ContribuyenteCedulaDto>(
                false,
                ApiResultStatusCode.ServerError,
                null,
                $"Unexpected error: {ex.Message}"
            );
        }
    }
}
```

### Complete Integration Example with All Services

```csharp
public class SriIntegrationService
{
    private readonly ISriClientService _sriClient;
    private readonly IRucService _rucService;
    private readonly ICedulaService _cedulaService;
    private readonly ILogger<SriIntegrationService> _logger;

    public SriIntegrationService(
        ISriClientService sriClient,
        IRucService rucService,
        ICedulaService cedulaService,
        ILogger<SriIntegrationService> logger)
    {
        _sriClient = sriClient;
        _rucService = rucService;
        _cedulaService = cedulaService;
        _logger = logger;
    }

    public async Task<IntegrationResult> ProcessCompleteDocumentAsync(
        string signedXml, 
        string issuerRuc, 
        string receiverCedula)
    {
        try
        {
            // 1. Validate that issuer exists
            var issuerResult = await _rucService.GetRucSriAsync(issuerRuc);
            if (!issuerResult.IsSuccess)
            {
                return IntegrationResult.CreateFailure($"Issuer not found: {issuerResult.Message}");
            }

            // 2. Validate that receiver exists (if natural person)
            if (!string.IsNullOrEmpty(receiverCedula))
            {
                var receiverResult = await _cedulaService.GetCedulaSriAsync(receiverCedula);
                if (!receiverResult.IsSuccess)
                {
                    return IntegrationResult.CreateFailure($"Receiver not found: {receiverResult.Message}");
                }
            }

            // 3. Process SRI document
            var validationResult = await _sriClient.ValidateDocumentAsync(signedXml);
            if (!validationResult.Success)
            {
                return IntegrationResult.CreateFailure($"Validation failed: {validationResult.ErrorMessage}");
            }

            var authResult = await _sriClient.RequestAuthorizationAsync(validationResult.AccessKey);
            if (!authResult.Success)
            {
                return IntegrationResult.CreateFailure($"Authorization failed: {authResult.ErrorMessage}");
            }

            return IntegrationResult.CreateSuccess(new
            {
                Issuer = issuerResult.Data,
                Receiver = receiverCedula,
                Authorization = authResult
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in complete processing");
            return IntegrationResult.CreateFailure($"Unexpected error: {ex.Message}");
        }
    }
}
```

## 🔒 Security Considerations

- **HTTPS Only**: Always use HTTPS for production environments
- **Certificate Validation**: Ensure proper certificate validation for SRI endpoints
- **Access Key Security**: Never log or expose access keys in plain text
- **Network Security**: Use secure network configurations
- **Logging**: Be careful not to log sensitive information

## 🧪 Testing

### Unit Testing Example

```csharp
[Test]
public async Task ValidateDocumentAsync_WithValidXml_ReturnsSuccess()
{
    // Arrange
    var mockLogger = new Mock<ILogger<SriClientService>>();
    var mockHttpClient = new Mock<HttpClient>();
    var service = new SriClientService(mockLogger.Object, mockHttpClient.Object);
    
    var signedXml = "<signed>document</signed>";

    // Act
    var result = await service.ValidateDocumentAsync(signedXml, SriEnvironment.Test);

    // Assert
    Assert.IsNotNull(result);
    // Add more specific assertions based on your test scenario
}
```

## 🚀 Performance

The service is optimized for high-performance operations:

- **Async Operations**: All I/O operations are asynchronous
- **Connection Pooling**: Uses HttpClient with connection pooling
- **Retry Logic**: Intelligent retry mechanism with exponential backoff
- **Timeout Management**: Configurable timeouts for different scenarios
- **Metrics**: Built-in performance monitoring

Typical performance metrics:
- Document validation: ~2-5 seconds
- Authorization request: ~1-3 seconds
- Retry scenarios: Additional 5-15 seconds depending on configuration

## 📦 Dependencies

- **.NET 8.0**: Target framework
- **Microsoft.Extensions.Configuration**: Configuration support
- **Microsoft.Extensions.DependencyInjection**: DI container support
- **Microsoft.Extensions.Http**: HttpClient factory
- **Microsoft.Extensions.Logging**: Logging infrastructure
- **Microsoft.Extensions.Options**: Options pattern support

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/yamgooo/Sri.Client/issues)
- **Documentation**: [Wiki](https://github.com/yamgooo)
- **Email**: erikportillapesantez@outlook.com

## 🙏 Credits

This project was inspired by and builds upon the work of the Acontplus team:

- **Acontplus RucService**: [https://github.com/Acontplus-S-A-S/acontplus-dotnet-libs/blob/main/src/Acontplus.FactElect/Services/Validation/RucService.cs](https://github.com/Acontplus-S-A-S/acontplus-dotnet-libs/blob/main/src/Acontplus.FactElect/Services/Validation/RucService.cs)

Special thanks to the Acontplus-S-A-S team for their original implementation and contribution to the Ecuadorian developer community.

---

**Made with ❤️ for the Ecuadorian developer community**
