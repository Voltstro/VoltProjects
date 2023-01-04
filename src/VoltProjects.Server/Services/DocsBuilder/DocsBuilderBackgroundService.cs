using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using VoltProjects.Server.Shared;

namespace VoltProjects.Server.Services.DocsBuilder;

public sealed class DocsBuilderBackgroundService : BackgroundService
{
    private readonly VoltProjectsConfig config;
    private readonly DocsBuilderService builderService;
    
    public DocsBuilderBackgroundService(IOptions<VoltProjectsConfig> mainConfig, DocsBuilderService builderService)
    {
        config = mainConfig.Value;
        this.builderService = builderService;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            builderService.UpdateOrBuildAllProjects(
                Path.Combine(AppContext.BaseDirectory, config.WorkingPath), 
                Path.Combine(AppContext.BaseDirectory, config.SitesServingDir));
            
            await Task.Delay(config.SitesRebuildTimeSeconds, stoppingToken);
        }
    }
}