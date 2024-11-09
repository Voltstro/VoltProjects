using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using VoltProjects.Builder.Core;
using VoltProjects.Builder.Services;
using VoltProjects.Shared;
using VoltProjects.Shared.Logging;
using VoltProjects.Shared.Services.Storage;
using VoltProjects.Shared.Telemetry;
using WebMarkupMin.Core;

//Create application
HostApplicationBuilder builder = Host.CreateApplicationBuilder();

//Setup logger
Logger logger = builder.Services.SetupLogger(builder.Configuration);

try
{
    builder.Services.AddTracking(builder.Configuration);
    
    //Setup Config
    VoltProjectsBuilderConfig config = new();
    IConfigurationSection configSection = builder.Configuration.GetSection("Config");
    configSection.Bind(config);
    builder.Services.Configure<VoltProjectsBuilderConfig>(configSection);
    
    //Our singletons
    builder.Services.InstallStorageServiceProvider(config.ObjectStorageProvider);
    builder.Services.AddSingleton<HtmlMinifier>();
    builder.Services.AddSingleton<HtmlHighlightService>();
    builder.Services.AddSingleton<ProjectRepoService>();
    builder.Services.AddSingleton<BuildManager>();

    //Background services
    builder.Services.AddHostedService<BuildService>();

    //Setup DB
    builder.Services.UseVoltProjectDbContext(builder.Configuration, "Builder");

    //Http Client
    builder.Services.AddHttpClient();

    //Setup app
    IHost host = builder.Build();

    //Handle DB migrations
    host.HandleDbMigrations();
    
    //Start
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Error(ex, "An uncaught error occured!");
#if DEBUG
    if (Debugger.IsAttached)
        throw;
#endif
    
    return 1;
}
finally
{
    logger.Dispose();
}

return 0;
