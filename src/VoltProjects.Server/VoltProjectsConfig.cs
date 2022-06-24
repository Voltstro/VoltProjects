namespace VoltProjects.Server;

public class VoltProjectsConfig
{
    public const string VoltProjects = "VoltProjects";
    
    public string SitesCacheDir { get; set; } = "Sites/";

    public int SitesUpdateTime { get; set; } = 128;

    public int HostCacheTime { get; set; } = 60 * 60 * 24 * 15;
}