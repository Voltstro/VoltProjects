using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Net.Http.Headers;
using OpenTelemetry.Trace;
using Serilog;
using VoltProjects.Server.Services;
using VoltProjects.Server.Shared;
using VoltProjects.Shared;
using VoltProjects.Shared.Logging;
using VoltProjects.Shared.Services.Storage;
using VoltProjects.Shared.Telemetry;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

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
    
    //Setup Config
    VoltProjectsConfig config = new();
    IConfigurationSection configSection = builder.Configuration.GetSection(VoltProjectsConfig.VoltProjects);
    configSection.Bind(config);
    
    //Setup services
    builder.Services.Configure<VoltProjectsConfig>(configSection);
    builder.Services.AddSingleton<StructuredDataService>();
    builder.Services.AddSingleton<SitemapService>();
    builder.Services.AddHostedService<SitemapBackgroundService>();
    builder.Services.AddSingleton<SearchService>();
    builder.Services.InstallStorageServiceProvider(config.ObjectStorageProvider);

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

    //Health Endpoints
    builder.Services.AddHealthChecks();
    
    //CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policyBuilder =>
        {
            policyBuilder
                .WithOrigins(config.CorsSites)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    builder.Services.Configure<RouteOptions>(options =>
    {
        options.AppendTrailingSlash = true;
    });
    
    //Auth
    OpenIdConfig openIdConfig = config.OpenIdConfig;
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.ExpireTimeSpan = openIdConfig.CookieExpiryTime;
            options.Cookie.SameSite = SameSiteMode.Lax;
        })
        .AddOpenIdConnect(options =>
        {
            //Signin Request
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.CallbackPath = "/auth/login/callback/";
            options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
            options.ResponseMode = OpenIdConnectResponseMode.FormPost;
            
            //Signout Request
            options.SignedOutCallbackPath = "/auth/logout/callback/";
            
            //OpenId details
            options.ClientId = openIdConfig.ClientId;
            options.ClientSecret = openIdConfig.ClientSecret;
            options.Authority = openIdConfig.Authority;
            options.ResponseType = "code id_token";
            
            options.UsePkce = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.MapInboundClaims = false;
            options.SaveTokens = true;

            foreach (string scope in openIdConfig.Scopes)
            {
                options.Scope.Add(scope);
            }
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
    
    //Static files
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
    
    //Custom Error Page
    app.UseStatusCodePagesWithReExecute("/Eroor/{0}");
    
    //Response Caching
    app.UseResponseCaching();
    
    //Routing
    app.UseRouting();
    
    //Security
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseCors();
    
    //Endpoints
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