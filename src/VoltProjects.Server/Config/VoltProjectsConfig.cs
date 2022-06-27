using System;

namespace VoltProjects.Server.Config;

public class VoltProjectsConfig
{
    public const string VoltProjects = "VoltProjects";

    public string SitesBuildDir { get; set; } = "Temp/";
    
    public string SitesServingDir { get; set; } = "Sites/";

    public int SitesUpdateTime { get; set; } = 60 * 60 * 24 * 15;

    public int HostCacheTime { get; set; } = 60 * 60 * 24 * 15;

    public VoltProject[] Projects { get; set; } = Array.Empty<VoltProject>();
}