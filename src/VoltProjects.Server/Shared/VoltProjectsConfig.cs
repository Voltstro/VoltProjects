using System;
using VoltProjects.Server.Models;

namespace VoltProjects.Server.Shared;

public sealed class VoltProjectsConfig
{
    public const string VoltProjects = "VoltProjects";
    
    public SocialLink[] SocialLinks { get; init; }= new SocialLink[]
    {
        new()
        {
            Name = "GitHub",
            Icon = "github",
            Href = "https://github.com/Voltstro"
        },
        new()
        {
            Name = "X (Formerly Twitter)",
            Icon = "twitter-x",
            Href = "https://x.com/Voltstro"
        },
        new()
        {
            Name = "Mastodon",
            Icon = "mastodon",
            Href = "https://mastodon.gamedev.place/@voltstro"
        },
        new()
        {
            Name = "Discord",
            Icon = "discord",
            Href = "https://discord.voltstro.dev"
        },
        new()
        {
            Name = "Website",
            Icon = "globe2",
            Href = "https://voltstro.dev"
        }
    };

    public bool FunnyMode { get; init; } = true;
    
    public string PublicUrl { get; init; }

    public TimeSpan SitemapGenerationDelayTime = new TimeSpan(6, 0, 0);
    
    public string[] CorsSites { get; init; }
    
    public string SiteUrl { get; init; }
    
    public int CacheTime { get; set; } = 2678400; //Default: 30 days
    
    public OpenIdConfig OpenIdConfig { get; init; }
}