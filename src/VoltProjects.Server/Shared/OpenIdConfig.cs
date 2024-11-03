using System;

namespace VoltProjects.Server.Shared;

public class OpenIdConfig
{
    public string ClientId { get; init; }
    
    public string ClientSecret { get; init; }
    
    public string Authority { get; init; }

    public string[] Scopes { get; init; } = new[] { "openid", "profile", "offline_access" };
    
    public TimeSpan CookieExpiryTime { get; init; } = TimeSpan.FromDays(1);
}