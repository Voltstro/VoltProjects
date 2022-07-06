using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using VoltProjects.DocsBuilder.Core;
using VoltProjects.DocsBuilder.DocFx;
using VoltProjects.Server.Core.Git;
using VoltProjects.Server.Core.MiddlewareManagement;
using VoltProjects.Server.Core.SiteCache;
using VoltProjects.Server.Core.SiteCache.Config;

WebApplicationBuilder builder = WebApplication.CreateBuilder();

//Setup logger
const string outPutTemplate = "{Timestamp:dd-MM hh:mm:ss tt} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
string logFileName =
    $"{AppContext.BaseDirectory}/Logs/{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.log";
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: outPutTemplate)
    .WriteTo.File(logFileName, outputTemplate: outPutTemplate)
    .CreateLogger();

try
{
    builder.WebHost.ConfigureKestrel(kestrel => kestrel.AddServerHeader = false);
    builder.Host.UseSerilog();

    //Setup services
    builder.Services.Configure<VoltProjectsConfig>(
        builder.Configuration.GetSection(VoltProjectsConfig.VoltProjects));

    builder.Services.AddControllersWithViews();
    builder.Services.AddResponseCaching();
    builder.Services.AddRuntimeMiddleware();

    //TODO: It be better if we could load doc builders dynamically
    builder.Services.AddSingleton(new DocsBuilder(new DocFxDocxBuilder()));
    builder.Services.AddSingleton<Git>();
    builder.Services.AddSingleton<SiteCacheManager>();
    builder.Services.AddHostedService<SitesCacheUpdater>();
    
    //Now setup the app
    WebApplication app = builder.Build();
    app.UseResponseCaching();
    app.UseRuntimeMiddleware();
    app.UseRouting();

    //Update our site cache now before running
    SiteCacheManager cacheManager = app.Services.GetRequiredService<SiteCacheManager>();
    cacheManager.UpdateCache();
    cacheManager.ConfigureFileServers();

    //Some configuration will change depending on the environment
    if (!app.Environment.IsDevelopment())
        app.UseHsts();
    else
        app.UseDeveloperExceptionPage();

    //Setup our views/controllers
    app.MapControllers();
    app.UseStaticFiles();

    app.Run();
}
catch (Exception ex)
{
    Log.Error(ex, "An uncaught error occured!");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;