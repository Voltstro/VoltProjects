using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OpenTelemetry.Trace;
using Serilog;
using VoltProjects.Server.Services;
using VoltProjects.Server.Shared;
using VoltProjects.Shared;
using VoltProjects.Shared.Logging;
using VoltProjects.Shared.Telemetry;

//Create application
WebApplicationBuilder builder = WebApplication.CreateBuilder();

//Setup logger
Logger logger = builder.Services.SetupLogger(builder.Configuration);

try
{
    //Setup web host
    builder.WebHost.ConfigureKestrel(kestrel => kestrel.AddServerHeader = false);

    //Setup tracking
    builder.Services.AddTracking(builder.Configuration, tracerBuilder =>
    {
        tracerBuilder.AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.Filter = context =>
            {
                //Filter out health requests
                bool isHealthRequest = context.Request.Path.Value.Contains("healthz");
                return !isHealthRequest;
            };
        });
    });

    //Setup services
    builder.Services.Configure<VoltProjectsConfig>(
        builder.Configuration.GetSection(VoltProjectsConfig.VoltProjects));
    builder.Services.AddSingleton<StructuredDataService>();
    builder.Services.AddSingleton<SitemapService>();
    builder.Services.AddHostedService<SitemapBackgroundService>();
    builder.Services.AddSingleton<SearchService>();

    //Support razor pages runtime compilation for hot reloading
    IMvcBuilder mvcBuilder = builder.Services.AddControllersWithViews(
        mvcOptions => mvcOptions.Filters.Add<OperationCancelledExceptionFilter>());

    //Allows for caching
    builder.Services.AddResponseCaching();
    
    //Development options
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        mvcBuilder.AddRazorRuntimeCompilation();
    }

    //Setup VoltProjects DB
    builder.Services.UseVoltProjectDbContext(builder.Configuration, "Server");

    builder.Services.AddHealthChecks();
    
    //CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policyBuilder =>
        {
            VoltProjectsConfig? config =
                builder.Configuration.GetSection(VoltProjectsConfig.VoltProjects).Get<VoltProjectsConfig>();

            policyBuilder.WithOrigins(config.CorsSites).AllowAnyHeader().AllowAnyMethod();
        });
    });

    //Now setup the app
    WebApplication app = builder.Build();
    
    //Handle DB migrations
    app.HandleDbMigrations();

    //Some configuration will change depending on the environment
    if (!app.Environment.IsDevelopment())
        app.UseHsts();
    else
        app.UseDeveloperExceptionPage();

    VoltProjectsConfig config = app.Services.GetRequiredService<IOptions<VoltProjectsConfig>>().Value;
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={config.CacheTime}";
        }
    });
    
    //Ensures '/' is added to the end of each request
    const string pattern = "^(((.*/)|(/?))[^/.]+(?!/$))$";
    RewriteOptions options = new RewriteOptions()
        .AddRedirect(pattern, "$1/",301);
    app.UseRewriter(options);
    
    app.UseStatusCodePagesWithReExecute("/Eroor/{0}");
    
    app.UseResponseCaching();
    app.UseRouting();
    
    //Use CORS
    app.UseCors();
    
    //Map main endpoints
    app.MapControllers();
    app.UseHealthChecks("/healthz/");

    Log.Information("Configuration done! Starting...");
    app.Run();
}
catch (Exception ex)
{
    Log.Error(ex, "An uncaught error occured!");
#if DEBUG
    if (Debugger.IsAttached)
        throw;
#endif
    
    return 1;
}
finally
{
    logger.Dispose();
}

return 0;