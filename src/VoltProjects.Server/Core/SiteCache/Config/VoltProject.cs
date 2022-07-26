using System;
using static System.String;

namespace VoltProjects.Server.Core.SiteCache.Config;

public class VoltProject
{
    public string Name { get; set; } = Empty;

    private string _gitPath = Empty;
    public string GitPath
    {
        get => _gitPath;
        set
        {
            _gitPath = value;
            if (value.StartsWith("https://"))
                GitIsRemote = true;
        }
    }

    public bool GitIsRemote { get; private set; }
    
    public string GitBranch { get; set; } = Empty;
    
    public bool GitUseLatestTag { get; set; }

    public string? IconPath { get; set; }
    
    public string DocsPath { get; set; } = Empty;
    public string DocsBuildDist { get; set; } = Empty;
}