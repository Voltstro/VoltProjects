using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace VoltProjects.Server.Shared.Helpers;

/// <summary>
///     Helper for caching
/// </summary>
public static class CachingHelper
{
    public static void SetCacheControl(this HttpContext httpContext, int cacheTime)
    {
        string cacheControl = $"public,max-age={cacheTime}";
        if (httpContext.User.Identity?.IsAuthenticated == true)
            cacheControl = "no-store,no-cache";

        httpContext.Response.Headers[HeaderNames.CacheControl] = cacheControl;
    }
}