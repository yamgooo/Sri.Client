// -------------------------------------------------------------
// By: Erik Portilla
// Date: 2025-01-27
// -------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yamgooo.SRI.Client.Contracts;
using Yamgooo.SRI.Client.Models;
using Yamgooo.SRI.Client.Services;

namespace Yamgooo.SRI.Client.Extensions;

/// <summary>
/// Extensions for configuring the SRI client service
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SRI client service with configuration from appsettings.json
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="sectionName">Section name in appsettings.json (default: "SriClient")</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddSriClientService(
        this IServiceCollection services, 
        IConfiguration configuration, 
        string sectionName = "SriClient")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        var configSection = configuration.GetSection(sectionName);
        if (!configSection.Exists())
        {
            throw new ArgumentException($"Configuration section '{sectionName}' not found in appsettings.json");
        }

        var serviceConfig = new SriServiceConfiguration();
        configSection.Bind(serviceConfig);

        if (!serviceConfig.IsValid())
        {
            var error = serviceConfig.GetValidationError();
            throw new ArgumentException($"Invalid SRI Client configuration: {error}");
        }

        services.Configure<SriServiceConfiguration>(options =>
        {
            options.TimeoutSeconds = serviceConfig.TimeoutSeconds;
            options.MaxRetries = serviceConfig.MaxRetries;
            options.RetryDelaySeconds = serviceConfig.RetryDelaySeconds;
        });

        // Register HttpClient
        services.AddHttpClient<ISriClientService, SriClientService>();

        // Register the service
        services.AddScoped<ISriClientService, SriClientService>();

        // Register additional SRI services
        services.AddScoped<ICookieService, CookieService>();
        services.AddScoped<ICaptchaService, CaptchaService>();
        services.AddScoped<ICedulaService, CedulaService>();
        services.AddScoped<IRucService, RucService>();

        return services;
    }

    /// <summary>
    /// Registers the SRI client service with custom configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Custom configuration</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddSriClientService(
        this IServiceCollection services, 
        SriServiceConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        if (!configuration.IsValid())
        {
            var error = configuration.GetValidationError();
            throw new ArgumentException($"Invalid SRI Client configuration: {error}");
        }

        services.Configure<SriServiceConfiguration>(options =>
        {
            options.TimeoutSeconds = configuration.TimeoutSeconds;
            options.MaxRetries = configuration.MaxRetries;
            options.RetryDelaySeconds = configuration.RetryDelaySeconds;
        });

        // Register HttpClient
        services.AddHttpClient<ISriClientService, SriClientService>();

        // Register the service
        services.AddScoped<ISriClientService, SriClientService>();

        // Register additional SRI services
        services.AddScoped<ICookieService, CookieService>();
        services.AddScoped<ICaptchaService, CaptchaService>();
        services.AddScoped<ICedulaService, CedulaService>();
        services.AddScoped<IRucService, RucService>();

        return services;
    }

    /// <summary>
    /// Registers the SRI client service with minimal configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="timeoutSeconds">Request timeout in seconds (default: 30)</param>
    /// <param name="maxRetries">Maximum number of retries (default: 3)</param>
    /// <param name="retryDelaySeconds">Delay between retries in seconds (default: 2)</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddSriClientService(
        this IServiceCollection services, 
        int timeoutSeconds = 30, 
        int maxRetries = 3, 
        int retryDelaySeconds = 2)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(timeoutSeconds);
        ArgumentOutOfRangeException.ThrowIfNegative(maxRetries);
        ArgumentOutOfRangeException.ThrowIfNegative(retryDelaySeconds);
        ArgumentNullException.ThrowIfNull(services);

        var configuration = new SriServiceConfiguration
        {
            TimeoutSeconds = timeoutSeconds,
            MaxRetries = maxRetries,
            RetryDelaySeconds = retryDelaySeconds
        };

        return services.AddSriClientService(configuration);
    }

    /// <summary>
    /// Registers the SRI client service without configuration (uses defaults)
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddSriClientService(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register HttpClient
        services.AddHttpClient<ISriClientService, SriClientService>();

        // Register the service with default configuration
        services.AddScoped<ISriClientService, SriClientService>();

        // Register additional SRI services
        services.AddScoped<ICookieService, CookieService>();
        services.AddScoped<ICaptchaService, CaptchaService>();
        services.AddScoped<ICedulaService, CedulaService>();
        services.AddScoped<IRucService, RucService>();

        return services;
    }

    /// <summary>
    /// Registers the SRI client service with specified lifetime
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="lifetime">Service lifetime</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddSriClientService(
        this IServiceCollection services, 
        ServiceLifetime lifetime)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register HttpClient
        services.AddHttpClient<ISriClientService, SriClientService>();

        // Register the service with specified lifetime
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton<ISriClientService, SriClientService>();
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped<ISriClientService, SriClientService>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<ISriClientService, SriClientService>();
                break;
            default:
                services.AddScoped<ISriClientService, SriClientService>();
                break;
        }

        // Register additional SRI services
        services.AddScoped<ICookieService, CookieService>();
        services.AddScoped<ICaptchaService, CaptchaService>();
        services.AddScoped<ICedulaService, CedulaService>();
        services.AddScoped<IRucService, RucService>();

        return services;
    }
}
