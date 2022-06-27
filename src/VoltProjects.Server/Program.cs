using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VoltProjects.DocsBuilder.Core;
using VoltProjects.DocsBuilder.DocFx;
using VoltProjects.Server;
using VoltProjects.Server.Config;
using VoltProjects.Server.Core.Git;
using VoltProjects.Server.SiteCache;

WebApplicationBuilder builder = WebApplication.CreateBuilder();
builder.WebHost.ConfigureKestrel(kestrel => kestrel.AddServerHeader = false);

IConfigurationSection config = builder.Configuration.GetSection(VoltProjectsConfig.VoltProjects);
builder.Services.Configure<VoltProjectsConfig>(config);
builder.Services.AddSingleton(new DocsBuilder(new DocFxDocxBuilder()));
builder.Services.AddSingleton<Git>();
builder.Services.AddSingleton<SiteCacheManager>();
builder.Services.AddHostedService<SitesCacheUpdater>();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseHsts();
else
    app.UseDeveloperExceptionPage();

app.UseRouting();

VoltProjectsConfig bindConfig = config.Get<VoltProjectsConfig>();
app.SetupVoltProjects(bindConfig);

//Update our site cache now before running
app.Services.GetService<SiteCacheManager>()!.UpdateCache();

app.Run();