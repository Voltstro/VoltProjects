using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace VoltProjects.Server.Core.MiddlewareManagement;

public class RuntimeMiddlewareService
{
    private Func<RequestDelegate, RequestDelegate>? _middleware;

    private IApplicationBuilder? _appBuilder;

    internal void Use(IApplicationBuilder app)
        => _appBuilder = app.Use(next => context => _middleware == null ? next(context) : _middleware(next)(context));

    public void Configure(Action<IApplicationBuilder> action)
    {
        if (_appBuilder == null)
            throw new ArgumentException($"The {nameof(RuntimeMiddlewareService)} has not been configured with an app yet!");
        
        IApplicationBuilder app = _appBuilder.New();
        action(app);
        _middleware = next => app.Use(_ => next).Build();
    }
}