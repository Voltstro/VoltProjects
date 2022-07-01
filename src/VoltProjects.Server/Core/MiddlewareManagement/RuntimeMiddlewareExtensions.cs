using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace VoltProjects.Server.Core.MiddlewareManagement;

/// <summary>
///     Provides the needed extensions to use the runtime middleware
/// </summary>
public static class RuntimeMiddlewareExtensions
{
    /// <summary>
    ///     Adds the <see cref="RuntimeMiddlewareService"/> to the services
    /// </summary>
    /// <param name="services"></param>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public static IServiceCollection AddRuntimeMiddleware(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        services.Add(new ServiceDescriptor(typeof(RuntimeMiddlewareService), typeof(RuntimeMiddlewareService), lifetime));
        return services;
    }

    /// <summary>
    ///     Tells the <see cref="RuntimeMiddlewareService"/> what <see cref="IApplicationBuilder"/> to use
    /// </summary>
    /// <param name="app"></param>
    /// <param name="defaultAction"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseRuntimeMiddleware(this IApplicationBuilder app, Action<IApplicationBuilder>? defaultAction = null)
    {
        RuntimeMiddlewareService service = app.ApplicationServices.GetRequiredService<RuntimeMiddlewareService>();
        service.Use(app);
        if (defaultAction != null)
        {
            service.Configure(defaultAction);
        }
        return app;
    }
}