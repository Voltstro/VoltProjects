using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sentry;
using Sentry.Extensions.Logging;
using Sentry.Extensions.Logging.Extensions.DependencyInjection;
using Sentry.OpenTelemetry;

namespace VoltProjects.Shared;

/// <summary>
///     Extensions for application tracking
/// </summary>
public static class Tracking
{
    public static readonly ActivitySource TrackingActivitySource = new("VoltProjects");
    
    public static void AddTracking(this IServiceCollection services, IConfiguration configuration, Action<TracerProviderBuilder>? configure = null)
    {
        //Setup Sentry Options
        CustomSentryOptions options = new();
        configuration.GetSection("Sentry").Bind(options);
        options.Dsn ??= string.Empty;
        
        services.AddSingleton(options);
        services.AddSingleton<ILoggerProvider, SentryLoggerProvider>();
        
        //Init Sentry
        SentrySdk.Init(options);
        services.AddSentry<CustomSentryOptions>();
        
        services.AddSingleton<MyResourceDetector>();
        services
            .AddOpenTelemetry()
            .ConfigureResource(builder =>
            {
                
                builder.AddDetector(sp => sp.GetRequiredService<MyResourceDetector>());
            })
            .WithTracing(builder =>
            {
                builder.AddSource("VoltProjects");
                builder.AddEntityFrameworkCoreInstrumentation(efOptions =>
                {
                    efOptions.EnrichWithIDbCommand = (activity, command) =>
                    {
                        string stateDisplayName = $"{command.CommandType} main";
                        activity.DisplayName = stateDisplayName;
                        activity.SetTag("db.name", stateDisplayName);
                        activity.SetTag("db.query", command.CommandText);
                    };
                });
                configure?.Invoke(builder);
                
                //Enable Sentry open telemetry
                if(SentrySdk.IsEnabled)
                    builder.AddSentry();
            });

        // All logs should flow to the SentryLogger, regardless of level.
        // Filtering of events is handled in SentryLogger, using SentryOptions.MinimumEventLevel
        // Filtering of breadcrumbs is handled in SentryLogger, using SentryOptions.MinimumBreadcrumbLevel
        //services.AddFilter<SentryLoggerProvider>(_ => true);
    }
}

internal class CustomSentryOptions : SentryLoggingOptions
{
    public CustomSentryOptions()
    {
        InitializeSdk = false;
        this.UseOpenTelemetry();
        this.AddEntityFramework();
    }
}

public class MyResourceDetector : IResourceDetector
{
    private readonly IHostEnvironment webHostEnvironment;

    public MyResourceDetector(IHostEnvironment webHostEnvironment)
    {
        this.webHostEnvironment = webHostEnvironment;
    }

    public Resource Detect()
    {
        return ResourceBuilder.CreateEmpty()
            .AddService(serviceName: this.webHostEnvironment.ApplicationName)
            .AddAttributes(new Dictionary<string, object> { ["host.environment"] = this.webHostEnvironment.EnvironmentName })
            .Build();
    }
}
