using System;

namespace VoltProjects.Server.Shared;

public class VoltProjectsConfig
{
    public const string VoltProjects = "VoltProjects";
    
    public string SitesServingDir { get; set; } = "Sites/";
    
    public int SitesServingCacheHeaderTimeSeconds { get; set; } = 60 * 60 * 24 * 15;

    public VoltProject[] Projects { get; set; } = Array.Empty<VoltProject>();
}