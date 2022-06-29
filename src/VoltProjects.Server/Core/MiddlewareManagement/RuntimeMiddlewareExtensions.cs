using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace VoltProjects.Server.Core.MiddlewareManagement;

public static class RuntimeMiddlewareExtensions
{
    public static IServiceCollection AddRuntimeMiddleware(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        services.Add(new ServiceDescriptor(typeof(RuntimeMiddlewareService), typeof(RuntimeMiddlewareService), lifetime));
        return services;
    }

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