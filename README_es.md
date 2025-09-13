# Servicio Cliente SRI

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![NuGet](https://img.shields.io/badge/nuget-Yamgooo.SRI.Client-blue.svg)](https://www.nuget.org/packages/Yamgooo.SRI.Client)

Una biblioteca profesional .NET para operaciones de cliente SRI (Servicio de Rentas Internas) en Ecuador. Este paquete proporciona integración perfecta con los servicios web del SRI para validación y autorización de documentos de facturación electrónica, así como consulta de información de contribuyentes.

También disponible en inglés: [README.md](README.md)

## 🚀 Características

- **Validación de Documentos**: Envía documentos XML firmados al SRI para validación y recepción
- **Solicitudes de Autorización**: Solicita autorización de documentos usando claves de acceso
- **Consulta de Contribuyentes**: Obtiene información completa de contribuyentes por RUC o cédula
- **Validación de Existencia**: Verifica automáticamente si los contribuyentes existen antes de obtener sus datos
- **Soporte de Entornos**: Soporte para entornos de prueba y producción del SRI
- **Operaciones Asíncronas**: Operaciones asíncronas de alto rendimiento
- **Soporte de Configuración**: Múltiples opciones de configuración (appsettings.json, código, dinámica)
- **Lógica de Reintento**: Mecanismo de reintento integrado con parámetros configurables
- **Registro de Eventos**: Registro completo con soporte de registro estructurado
- **Manejo de Errores**: Manejo robusto de errores con mensajes detallados
- **Monitoreo de Rendimiento**: Métricas de rendimiento y temporización integradas

## 📦 Instalación

### Paquete NuGet
```bash
dotnet add package Yamgooo.SRI.Client
```

### Instalación Manual
```bash
git clone https://github.com/yamgooo/Sri.Client.git
cd Sri.Client
dotnet build
```

## 🛠️ Inicio Rápido

### 1. Uso Básico (Configuración por Defecto)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Yamgooo.SRI.Client;

var services = new ServiceCollection();

// Registrar el servicio con configuración por defecto
services.AddLogging();
services.AddSriClientService();

var provider = services.BuildServiceProvider();
var sriClient = provider.GetRequiredService<ISriClientService>();

// Validar un documento firmado
var validationResult = await sriClient.ValidateDocumentAsync(signedXml, SriEnvironment.Test);

if (validationResult.Success)
{
    Console.WriteLine($"Documento validado exitosamente. Clave de Acceso: {validationResult.AccessKey}");
    
    // Solicitar autorización
    var authResult = await sriClient.RequestAuthorizationAsync(validationResult.AccessKey, SriEnvironment.Test);
    
    if (authResult.Success)
    {
        Console.WriteLine($"Documento autorizado. Número: {authResult.AuthorizationNumber}");
    }
}
```

### 2. Uso con Configuración

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
// Registrar con configuración
services.AddSriClientService(configuration);

// Usar el servicio
var sriClient = serviceProvider.GetRequiredService<ISriClientService>();
var result = await sriClient.ValidateDocumentAsync(signedXml);
```

### 3. Configuración Personalizada

```csharp
var config = new SriServiceConfiguration
{
    TimeoutSeconds = 180,
    MaxRetries = 5,
    RetryDelaySeconds = 10
};

services.AddSriClientService(config);
```

## 📋 Referencia de API

### Interfaz ISriClientService

#### Validación de Documentos
```csharp
Task<SriValidationResult> ValidateDocumentAsync(string signedXml, SriEnvironment environment = SriEnvironment.Test);
```

#### Solicitud de Autorización
```csharp
Task<SriAuthorizationResult> RequestAuthorizationAsync(string accessKey, SriEnvironment environment = SriEnvironment.Test);
```

#### Gestión de Configuración
```csharp
SriServiceConfiguration GetConfiguration();
void UpdateConfiguration(SriServiceConfiguration configuration);
```

### Interfaz IRucService

#### Consulta de Contribuyente por RUC
```csharp
Task<ContribuyenteCompleteDto> GetRucSriAsync(string ruc);
```

**Características:**
- Valida automáticamente que el RUC tenga 13 dígitos numéricos
- Verifica la existencia del contribuyente en el SRI antes de obtener datos
- Retorna información completa del contribuyente incluyendo establecimientos
- Lanza excepciones apropiadas en caso de error

### Interfaz ICedulaService

#### Consulta de Contribuyente por Cédula
```csharp
Task<ContribuyenteCedulaDto> GetCedulaSriAsync(string cedula);
```

**Características:**
- Valida automáticamente que la cédula tenga 10 dígitos numéricos
- Verifica la existencia del contribuyente en el Registro Civil antes de obtener datos
- Retorna información básica del contribuyente (identificación, nombre completo, fecha de defunción)
- Lanza excepciones apropiadas en caso de error

### Modelos de Resultado

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

## 🔧 Opciones de Configuración

### Métodos de Registro de Servicio

```csharp
// Desde appsettings.json
services.AddSriClientService(configuration, "SriClient");

// Con objeto de configuración personalizado
services.AddSriClientService(customConfig);

// Con parámetros directos
services.AddSriClientService(timeoutSeconds: 120, maxRetries: 3, retryDelaySeconds: 5);

// Sin configuración (usa valores por defecto)
services.AddSriClientService();
```

### Propiedades de Configuración

- **TimeoutSeconds**: Tiempo de espera de la solicitud en segundos (por defecto: 120)
- **MaxRetries**: Número máximo de reintentos (por defecto: 3)
- **RetryDelaySeconds**: Retraso entre reintentos en segundos (por defecto: 5)

## 📝 Ejemplos

### Ejemplo Completo de Procesamiento de Documento SRI

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
            // Paso 1: Validar documento
            _logger.LogInformation("Iniciando validación de documento");
            var validationResult = await _sriClient.ValidateDocumentAsync(signedXml, environment);
            
            if (!validationResult.Success)
            {
                _logger.LogError("Falló la validación del documento: {ErrorMessage}", validationResult.ErrorMessage);
                return ProcessingResult.CreateFailure(validationResult.ErrorMessage);
            }

            _logger.LogInformation("Documento validado exitosamente. Clave de Acceso: {AccessKey}", validationResult.AccessKey);

            // Paso 2: Solicitar autorización
            _logger.LogInformation("Solicitando autorización del documento");
            var authResult = await _sriClient.RequestAuthorizationAsync(validationResult.AccessKey, environment);
            
            if (!authResult.Success)
            {
                _logger.LogError("Falló la autorización del documento: {ErrorMessage}", authResult.ErrorMessage);
                return ProcessingResult.CreateFailure(authResult.ErrorMessage);
            }

            _logger.LogInformation("Documento autorizado exitosamente. Número: {AuthNumber}", authResult.AuthorizationNumber);
            
            return ProcessingResult.CreateSuccess(authResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando documento SRI");
            return ProcessingResult.CreateFailure(ex.Message);
        }
    }
}
```

### Ejemplo de Consulta de Contribuyente por RUC

```csharp
public class ContribuyenteService
{
    private readonly IRucService _rucService;
    private readonly ILogger<ContribuyenteService> _logger;

    public ContribuyenteService(IRucService rucService, ILogger<ContribuyenteService> logger)
    {
        _rucService = rucService;
        _logger = logger;
    }

    public async Task<ContribuyenteCompleteDto> ConsultarContribuyentePorRucAsync(string ruc)
    {
        try
        {
            _logger.LogInformation("Consultando contribuyente por RUC: {Ruc}", ruc);
            
            var result = await _rucService.GetRucSriAsync(ruc);
            
            _logger.LogInformation("Contribuyente encontrado: {Nombre}", result.Contribuyente.RazonSocial);
            _logger.LogInformation("Establecimientos encontrados: {Count}", result.Establecimientos.Count);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando contribuyente por RUC: {Ruc}", ruc);
            throw;
        }
    }
}
```

### Ejemplo de Consulta de Contribuyente por Cédula

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

    public async Task<ContribuyenteCedulaDto> ConsultarContribuyentePorCedulaAsync(string cedula)
    {
        try
        {
            _logger.LogInformation("Consultando contribuyente por cédula: {Cedula}", cedula);
            
            var result = await _cedulaService.GetCedulaSriAsync(cedula);
            
            _logger.LogInformation("Contribuyente encontrado: {Nombre}", result.NombreCompleto);
            
            if (!string.IsNullOrEmpty(result.FechaDefuncion))
            {
                _logger.LogWarning("Contribuyente fallecido: {FechaDefuncion}", result.FechaDefuncion);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando contribuyente por cédula: {Cedula}", cedula);
            throw;
        }
    }
}
```

### Ejemplo de Uso Completo con Todos los Servicios

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

    public async Task<IntegrationResult> ProcesarDocumentoCompletoAsync(
        string signedXml, 
        string rucEmisor, 
        string cedulaReceptor)
    {
        try
        {
            // 1. Validar que el emisor existe
            var emisorResult = await _rucService.GetRucSriAsync(rucEmisor);
            if (!emisorResult.IsSuccess)
            {
                return IntegrationResult.CreateFailure($"Emisor no encontrado: {emisorResult.Message}");
            }

            // 2. Validar que el receptor existe (si es persona natural)
            if (!string.IsNullOrEmpty(cedulaReceptor))
            {
                var receptorResult = await _cedulaService.GetCedulaSriAsync(cedulaReceptor);
                if (!receptorResult.IsSuccess)
                {
                    return IntegrationResult.CreateFailure($"Receptor no encontrado: {receptorResult.Message}");
                }
            }

            // 3. Procesar documento SRI
            var validationResult = await _sriClient.ValidateDocumentAsync(signedXml);
            if (!validationResult.Success)
            {
                return IntegrationResult.CreateFailure($"Validación fallida: {validationResult.ErrorMessage}");
            }

            var authResult = await _sriClient.RequestAuthorizationAsync(validationResult.AccessKey);
            if (!authResult.Success)
            {
                return IntegrationResult.CreateFailure($"Autorización fallida: {authResult.ErrorMessage}");
            }

            return IntegrationResult.CreateSuccess(new
            {
                Emisor = emisorResult.Data,
                Receptor = cedulaReceptor,
                Autorizacion = authResult
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en procesamiento completo");
            return IntegrationResult.CreateFailure($"Error inesperado: {ex.Message}");
        }
    }
}
```

## 🔒 Consideraciones de Seguridad

- **Solo HTTPS**: Siempre usa HTTPS para entornos de producción
- **Validación de Certificados**: Asegura la validación adecuada de certificados para endpoints del SRI
- **Seguridad de Claves de Acceso**: Nunca registres o expongas claves de acceso en texto plano
- **Seguridad de Red**: Usa configuraciones de red seguras
- **Registro de Eventos**: Ten cuidado de no registrar información sensible

## 🧪 Pruebas

### Ejemplo de Prueba Unitaria

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
    // Agregar más aserciones específicas basadas en tu escenario de prueba
}
```

## 🚀 Rendimiento

El servicio está optimizado para operaciones de alto rendimiento:

- **Operaciones Asíncronas**: Todas las operaciones de I/O son asíncronas
- **Agrupación de Conexiones**: Usa HttpClient con agrupación de conexiones
- **Lógica de Reintento**: Mecanismo de reintento inteligente con retroceso exponencial
- **Gestión de Tiempo de Espera**: Tiempos de espera configurables para diferentes escenarios
- **Métricas**: Monitoreo de rendimiento integrado

Métricas típicas de rendimiento:
- Validación de documento: ~2-5 segundos
- Solicitud de autorización: ~1-3 segundos
- Escenarios de reintento: 5-15 segundos adicionales dependiendo de la configuración

## 📦 Dependencias

- **.NET 8.0**: Framework objetivo
- **Microsoft.Extensions.Configuration**: Soporte de configuración
- **Microsoft.Extensions.DependencyInjection**: Soporte de contenedor DI
- **Microsoft.Extensions.Http**: Factory de HttpClient
- **Microsoft.Extensions.Logging**: Infraestructura de registro
- **Microsoft.Extensions.Options**: Soporte del patrón de opciones

## 🤝 Contribuir

1. Haz un fork del repositorio
2. Crea una rama de características (`git checkout -b feature/amazing-feature`)
3. Confirma tus cambios (`git commit -m 'Add some amazing feature'`)
4. Empuja a la rama (`git push origin feature/amazing-feature`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto está licenciado bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para detalles.

## 📞 Soporte

- **Issues**: [GitHub Issues](https://github.com/yamgooo/Sri.Client/issues)
- **Documentación**: [Wiki](https://github.com/yamgooo)
- **Email**: erikportillapesantez@outlook.com

---

**Hecho con ❤️ para la comunidad de desarrolladores ecuatorianos**

