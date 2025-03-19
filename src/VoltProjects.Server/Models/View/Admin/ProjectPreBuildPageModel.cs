using System;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Models.View.Admin;

public class ProjectPreBuildPageModel : ProjectPreBuild
{
    public ProjectPreBuildPageModel()
    {
    }
    
    public ProjectPreBuildPageModel(ProjectPreBuild preBuild)
    {
        Id = preBuild.Id;
        ProjectVersionId = preBuild.ProjectVersionId;
        ProjectVersion = preBuild.ProjectVersion;
        Order = preBuild.Order;
        Command = preBuild.Command;
        Arguments = preBuild.Arguments;
        CreationTime = preBuild.CreationTime;
    }
    
    public new ProjectVersion? ProjectVersion { get; set; }
    
    public new DateTime? LastUpdateTime { get; set; }
    
    public bool New { get; set; }
    
    public bool Deleted { get; set; }
}