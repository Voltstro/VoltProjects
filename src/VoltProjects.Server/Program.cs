using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VoltProjects.Server;

WebApplicationBuilder builder = WebApplication.CreateBuilder();

IConfigurationSection config = builder.Configuration.GetSection(VoltProjectsConfig.VoltProjects);
builder.Services.Configure<VoltProjectsConfig>(config);
builder.Services.AddHostedService<SitesCacheUpdater>();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseHsts();
else
    app.UseDeveloperExceptionPage();

app.UseRouting();

VoltProjectsConfig bindConfig = config.Get<VoltProjectsConfig>();
app.SetupVoltProjects(bindConfig);

app.Run();