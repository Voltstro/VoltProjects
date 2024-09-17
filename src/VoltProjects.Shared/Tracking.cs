using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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
        IConfigurationSection optionsSection = configuration.GetSection("Sentry");
        optionsSection.Bind(options);
        if (string.IsNullOrWhiteSpace(options.Dsn))
            return;
        
        //Install open telemetry
        services.AddSingleton<MyResourceDetector>();
        services.AddOpenTelemetry()
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
                builder.AddHttpClientInstrumentation();
                
                configure?.Invoke(builder);

                builder.AddSentry();
            });
        
        //Install Sentry
        services.Configure<CustomSentryOptions>(optionsSection);
        services.AddSentry<CustomSentryOptions>();
    }
}

internal class CustomSentryOptions : SentryLoggingOptions
{
    public CustomSentryOptions()
    {
        InitializeSdk = true;
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
