using System;
using static System.String;

namespace VoltProjects.Server.Core.SiteCache.Config;

public class VoltProject
{
    public string Name { get; set; } = Empty;

    public Uri GitUrl { get; set; } = new("https://rowansuxlol.com");
    public string GitBranch { get; set; } = Empty;
    
    public bool GitUseLatestTag { get; set; }

    public string DocsPath { get; set; } = Empty;
    public string DocsBuildDist { get; set; } = Empty;
}