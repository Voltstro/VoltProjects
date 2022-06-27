using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoltProjects.Server.Config;
using VoltProjects.Server.SiteCache;

namespace VoltProjects.Server;

/// <summary>
///     Background services that is responsible for updating the sites
/// </summary>
public class SitesCacheUpdater : BackgroundService
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
            //Now you might think "should we immediately update before delaying?" however we call to the SiteCacheManager
            //to build the cache before running the app
            await Task.Delay(_config.SitesUpdateTime, stoppingToken);
            
            _logger.LogInformation("Updating site cache...");

            
        }
    }
}