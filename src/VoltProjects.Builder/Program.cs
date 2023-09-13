using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using VoltProjects.Builder.Core;
using VoltProjects.Builder.Services;
using VoltProjects.Builder.Services.Storage;
using VoltProjects.Shared;
using VoltProjects.Shared.Logging;
using WebMarkupMin.Core;
using IStorageService = VoltProjects.Builder.Services.Storage.IStorageService;

//Create application
HostApplicationBuilder builder = Host.CreateApplicationBuilder();

//Setup logger
Logger logger = builder.Services.SetupLogger(builder.Configuration);

try
{
    //Setup Config
    builder.Services.Configure<VoltProjectsBuilderConfig>(builder.Configuration.GetSection("Config"));

    //Our singletons
    builder.Services.AddSingleton<IStorageService, AzureStorageService>();
    builder.Services.AddSingleton<HtmlMinifier>();
    builder.Services.AddSingleton<HtmlHighlightService>();
    builder.Services.AddSingleton<ProjectRepoService>();
    builder.Services.AddSingleton<BuildManager>();

    //Background services
    builder.Services.AddHostedService<BuildService>();

    //Setup DB
    builder.Services.UseVoltProjectDbContext(builder.Configuration);

    //Setup app
    IHost host = builder.Build();
    
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
