using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;
using VoltProjects.Server.Services;
using VoltProjects.Server.Shared;
using VoltProjects.Shared;
using VoltProjects.Tests.Utils.Memory;

namespace VoltProjects.Tests.Utils;

/// <summary>
///     Utils for creating server's services
/// </summary>
public static class ServiceUtils
{
    /// <summary>
    ///     Creates a testing <see cref="ProjectService"/>
    /// </summary>
    /// <returns></returns>
    public static ProjectService CreateProjectService(VoltProjectDbContext dbContext)
    {
        IMemoryCache memoryCache = new NullMemoryCache();

        IOptions<VoltProjectsConfig> options = ConfigUtils.CreateServerConfig();
        return new ProjectService(dbContext, memoryCache, options);
    }

    /// <summary>
    ///     Creates a testing <see cref="SitemapService"/>
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public static SitemapService CreateSitemapService(VoltProjectDbContext dbContext)
    {
        IMemoryCache memoryCache = new NullMemoryCache();

        IOptions<VoltProjectsConfig> options = ConfigUtils.CreateServerConfig();
        return new SitemapService(dbContext, memoryCache, options);
    }
}