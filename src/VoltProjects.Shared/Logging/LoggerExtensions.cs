using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace VoltProjects.Shared.Logging;

/// <summary>
///     Shared setup methods for logging across VoltProjects
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    ///     Setups the application's logger
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static Logger SetupLogger(this IServiceCollection services, IConfiguration configuration)
    {
        Logger newLogger = new(configuration);
        services.AddSerilog();
        
        return newLogger;
    }
}