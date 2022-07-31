using System;
using System.Diagnostics;
using Figgle;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Serilog;
using VoltProjects.DocsBuilder.Core;
using VoltProjects.Server.Core.Git;
using VoltProjects.Server.Core.MiddlewareManagement;
using VoltProjects.Server.Core.Robots;
using VoltProjects.Server.Core.SiteCache;
using VoltProjects.Server.Core.SiteCache.Config;

WebApplicationBuilder builder = WebApplication.CreateBuilder();

//Setup logger
const string outPutTemplate = "{Timestamp:dd-MM hh:mm:ss tt} [{Level:u3}] [T: {ThreadId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
string logFileName =
    $"{AppContext.BaseDirectory}/Logs/{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.log";
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: outPutTemplate)
    .WriteTo.File(logFileName, outputTemplate: outPutTemplate)
    .CreateLogger();

//NOTE: We are adding a new line due to all the stuff at the start of each log message
Log.Information($"\n{FiggleFonts.Graffiti.Render("VoltProjects")}");
Log.Information("Logger started...");

try
{
    //Setup host
    builder.Host.UseSerilog();
    builder.WebHost.ConfigureKestrel(kestrel => kestrel.AddServerHeader = false);

    //Setup services
    builder.Services.Configure<VoltProjectsConfig>(
        builder.Configuration.GetSection(VoltProjectsConfig.VoltProjects));

    //Support razor pages runtime compilation for hot reloading
    IMvcBuilder mvcBuilder = builder.Services.AddControllersWithViews();
#if DEBUG
    if (builder.Environment.IsDevelopment())
        mvcBuilder.AddRazorRuntimeCompilation();
#endif

    //Allows for caching
    builder.Services.AddResponseCaching();

    //VoltProject's services
    builder.Services.AddRuntimeMiddleware();
    builder.Services.AddSitemapService();
    builder.Services.AddSingleton(new DocsBuilderManager(DependencyContext.Default));
    builder.Services.AddSingleton<Git>();
    builder.Services.AddSingleton<SiteCacheManager>();
    builder.Services.AddHostedService<SitesCacheUpdater>();
    
    //Now setup the app
    WebApplication app = builder.Build();
    app.UseResponseCaching();
    app.UseRuntimeMiddleware();
    app.UseRouting();
    app.UseStatusCodePagesWithReExecute("/Eroor/{0}");
    app.UseSitemapMiddleware();
    
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
#if DEBUG
    if (Debugger.IsAttached)
        throw;
#endif
    
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;