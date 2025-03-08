using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Models.View.Admin;

/// <summary>
///     View model for the /project/edit|new/ page.
/// </summary>
public class ProjectPageModel : Project
{
    public ProjectPageModel()
    {
    }
    
    public ProjectPageModel(Project project)
    {
        Id = project.Id;
        Name = project.Name;
        DisplayName = project.DisplayName;
        ShortName = project.ShortName;
        Description = project.Description;
        GitUrl = project.GitUrl;
        IconPath = project.IconPath;
        LastUpdateTime = project.LastUpdateTime;
        CreationTime = project.CreationTime;
        ProjectVersions = project.ProjectVersions;
    }
    
    public new int? Id { get; set; }
    
    public new ICollection<ProjectVersion>? ProjectVersions { get; set; }
    
    public IFormFile? ProjectIcon { get; set; }

    public bool? Success { get; set; }
}