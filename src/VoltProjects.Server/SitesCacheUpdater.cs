using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VoltProjects.Server;

/// <summary>
///     Background services that is responsible for updating the sites
/// </summary>
public class SitesCacheUpdater : BackgroundService
{
    private readonly ILogger<SitesCacheUpdater> _logger;

    public SitesCacheUpdater(ILogger<SitesCacheUpdater> logger)
    {
        _logger = logger;
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Hello World!");
        return Task.CompletedTask;
    }
}