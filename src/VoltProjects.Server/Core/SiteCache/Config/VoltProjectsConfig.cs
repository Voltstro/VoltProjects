using System;

namespace VoltProjects.Server.Core.SiteCache.Config;

public class VoltProjectsConfig
{
    public const string VoltProjects = "VoltProjects";

    public string SitesBuildDir { get; set; } = "Temp/";
    
    public string SitesServingDir { get; set; } = "Sites/";

    public bool CleanupOnError { get; set; } = true;

    public int SitesRebuildTimeSeconds { get; set; } = 60 * 60 * 24 * 15;

    public int SitesServingCacheHeaderTimeSeconds { get; set; } = 60 * 60 * 24 * 15;

    public VoltProject[] Projects { get; set; } = Array.Empty<VoltProject>();
}