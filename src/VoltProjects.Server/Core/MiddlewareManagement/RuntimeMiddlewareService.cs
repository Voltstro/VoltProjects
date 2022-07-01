using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace VoltProjects.Server.Core.MiddlewareManagement;

/// <summary>
///     Allows for middlewares to be added to a <see cref="IApplicationBuilder"/> at runtime
/// </summary>
public class RuntimeMiddlewareService
{
    private Func<RequestDelegate, RequestDelegate>? _middleware;

    private IApplicationBuilder? _appBuilder;

    internal void Use(IApplicationBuilder app)
        => _appBuilder = app.Use(next => context => _middleware == null ? next(context) : _middleware(next)(context));

    /// <summary>
    ///     Call when you want to add more middlewares to the <see cref="IApplicationBuilder"/>
    /// </summary>
    /// <param name="action"></param>
    /// <exception cref="ArgumentException"></exception>
    public void Configure(Action<IApplicationBuilder> action)
    {
        if (_appBuilder == null)
            throw new ArgumentException($"The {nameof(RuntimeMiddlewareService)} has not been configured with an app yet!");
        
        IApplicationBuilder app = _appBuilder.New();
        action(app);
        _middleware = next => app.Use(_ => next).Build();
    }
}