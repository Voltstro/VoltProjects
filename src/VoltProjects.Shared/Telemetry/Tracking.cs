using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sentry.Extensions.Logging;
using Sentry.Extensions.Logging.Extensions.DependencyInjection;
using Sentry.OpenTelemetry;

namespace VoltProjects.Shared.Telemetry;

/// <summary>
///     Extensions for application tracking
/// </summary>
public static class Tracking
{
    private static readonly ActivitySource TrackingActivitySource = new("VoltProjects");

    /// <summary>
    ///     Starts a new tracking activity
    /// </summary>
    /// <param name="area"></param>
    /// <param name="name"></param>
    /// <param name="activityKind"></param>
    /// <param name="tags"></param>
    /// <returns></returns>
    public static Activity StartActivity(string area, string name, ActivityKind activityKind = ActivityKind.Internal, IEnumerable<KeyValuePair<string, object?>>? tags = null)
    {
        return TrackingActivitySource.StartActivity(name: $"{area}.{name}", kind: activityKind, tags: tags)!;
    }
    
    /// <summary>
    ///     Installs tracking to services
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="configure"></param>
    public static void AddTracking(this IServiceCollection services, IConfiguration configuration, Action<TracerProviderBuilder>? configure = null)
    {
        //Get main tracking config
        TrackingConfig config = new();
        IConfigurationSection configSection = configuration.GetSection(TrackingConfig.SectionName);
        configSection.Bind(config);
        bool otlpEnabled = !string.IsNullOrWhiteSpace(config.OtlpEndpoint);
        
        //Also get sentry options
        CustomSentryOptions sentryConfig = new();
        IConfigurationSection sentryConfigSection = configSection.GetSection("Sentry");
        sentryConfigSection.Bind(sentryConfig);
        bool sentryEnabled = !string.IsNullOrWhiteSpace(sentryConfig.Dsn);
        
        //Neither sentry nor otlp is enabled
        if(!otlpEnabled && !sentryEnabled)
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
                
                //Otlp
                if (otlpEnabled)
                {
                    builder.AddOtlpExporter(otlp =>
                    {
                        otlp.Endpoint = new Uri(config.OtlpEndpoint!);
                        otlp.Headers = config.OtlpHeaders;
                    });
                }

                //Sentry
                if(sentryEnabled)
                    builder.AddSentry();
            });
        
        //Install Sentry
        if (sentryEnabled)
        {
            services.Configure<CustomSentryOptions>(sentryConfigSection);
            services.AddSentry<CustomSentryOptions>();
        }
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
