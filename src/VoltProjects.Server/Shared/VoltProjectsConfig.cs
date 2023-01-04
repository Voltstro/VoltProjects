using System;

namespace VoltProjects.Server.Shared;

public sealed class VoltProjectsConfig
{
    public const string VoltProjects = "VoltProjects";
    
    public string SitesServingDir { get; set; } = "Sites/";

    public string WorkingPath { get; set; } = "Working/";
    
    public int SitesRebuildTimeSeconds { get; set; }
    
    public int SitesServingCacheHeaderTimeSeconds { get; set; } = 60 * 60 * 24 * 15;

    public VoltProject[] Projects { get; set; } = Array.Empty<VoltProject>();
}