using System;

namespace VoltProjects.Server.Core.SiteCache.Config;

public struct VoltProject
{
    public string Name { get; set; }
    
    public Uri GitUrl { get; set; }
    public string GitBranch { get; set; }
    
    public bool GitUseLatestTag { get; set; }
    
    public string DocsPath { get; set; }
    public string DocsBuildDist { get; set; }
}