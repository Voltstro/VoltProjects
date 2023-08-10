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
            Name = "Twitter",
            Icon = "twitter",
            Href = "https://twitter.com/Voltstro"
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
}