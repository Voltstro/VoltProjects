using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoltProjects.Server.Core.SiteCache.Config;

namespace VoltProjects.Server.Core.SiteCache;

/// <summary>
///     Background services that is responsible for updating the sites
/// </summary>
public sealed class SitesCacheUpdater : BackgroundService
{
    private readonly ILogger<SitesCacheUpdater> _logger;
    private readonly VoltProjectsConfig _config;
    private readonly SiteCacheManager _cacheManager;

    public SitesCacheUpdater(ILogger<SitesCacheUpdater> logger, IOptions<VoltProjectsConfig> config, SiteCacheManager cacheManager)
    {
        _logger = logger;
        _config = config.Value;
        _cacheManager = cacheManager;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //Now you might think "shouldn't we immediately call update cache before delaying?" however we call UpdateCache
            //before WebApplication's run is called. (So before this is executed)
            await Task.Delay(_config.SitesRebuildTimeSeconds * 1000, stoppingToken);
            
            _logger.LogInformation("Updating site cache...");
            _cacheManager.UpdateCache();
        }
    }
}