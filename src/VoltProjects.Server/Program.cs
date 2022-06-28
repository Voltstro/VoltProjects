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
using VoltProjects.Server.SiteCache;

WebApplicationBuilder builder = WebApplication.CreateBuilder();
builder.WebHost.ConfigureKestrel(kestrel => kestrel.AddServerHeader = false);
builder.WebHost.UseSentry();
builder.Host.UseSerilog();

//Setup services
IConfigurationSection config = builder.Configuration.GetSection(VoltProjectsConfig.VoltProjects);
builder.Services.Configure<VoltProjectsConfig>(config);
builder.Services.AddSingleton(new DocsBuilder(new DocFxDocxBuilder()));
builder.Services.AddSingleton<Git>();
builder.Services.AddSingleton<SiteCacheManager>();
builder.Services.AddHostedService<SitesCacheUpdater>();
builder.Services.AddMvc(options => options.EnableEndpointRouting = false);

//Setup logger
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
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

VoltProjectsConfig bindConfig = config.Get<VoltProjectsConfig>();
app.SetupVoltProjects(bindConfig);

//Update our site cache now before running
app.Services.GetService<SiteCacheManager>()!.UpdateCache();

app.UseStaticFiles();
app.UseSentryTracing();
app.Run();
Log.CloseAndFlush();