using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using VoltProjects.DocsBuilder.Core;
using VoltProjects.DocsBuilder.DocFx;
using VoltProjects.Server;
using VoltProjects.Server.Config;
using VoltProjects.Server.Core.Git;
using VoltProjects.Server.Core.MiddlewareManagement;
using VoltProjects.Server.SiteCache;

WebApplicationBuilder builder = WebApplication.CreateBuilder();
builder.WebHost.ConfigureKestrel(kestrel => kestrel.AddServerHeader = false);
builder.Host.UseSerilog();

//Setup services
IConfigurationSection config = builder.Configuration.GetSection(VoltProjectsConfig.VoltProjects);
builder.Services.Configure<VoltProjectsConfig>(config);
builder.Services.AddRuntimeMiddleware();
builder.Services.AddSingleton(new DocsBuilder(new DocFxDocxBuilder()));
builder.Services.AddSingleton<Git>();
builder.Services.AddSingleton<SiteCacheManager>();
builder.Services.AddHostedService<SitesCacheUpdater>();
builder.Services.AddMvc(options => options.EnableEndpointRouting = false);

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

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseHsts();
else
    app.UseDeveloperExceptionPage();

app.UseMvc(route =>
{
    route.MapRoute("default", "{controller=Main}/{action=Index}");
});

app.UseRouting();
app.UseRuntimeMiddleware();

//Update our site cache now before running
app.Services.GetService<SiteCacheManager>()!.UpdateCache();

app.UseStaticFiles();
app.Run();
Log.CloseAndFlush();