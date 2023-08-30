using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VoltProjects.Shared;

/// <summary>
///     VoltProject <see cref="DbContext"/> extensions
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    ///     Adds <see cref="VoltProjectDbContext"/> to a <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection UseVoltProjectDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddDbContextFactory<VoltProjectDbContext>(
                options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")))
            .AddDbContext<VoltProjectDbContext>(
                options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
    }
}