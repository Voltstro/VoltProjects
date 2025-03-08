using System.Collections.Generic;
using System.IO;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Models.View;

/// <summary>
///     View Model for ProjectNav
/// </summary>
public sealed class ProjectNavModel
{
    public ProjectNavModel(ProjectVersion projectVersion, IReadOnlyList<MenuItem> menuItems)
    {
        string baseProjectPath = $"/{Path.Combine(projectVersion.Project.Name, projectVersion.VersionTag)}";
        
        ProjectId = projectVersion.ProjectId;
        ProjectVersionId = projectVersion.Id;
        ProjectName = projectVersion.Project.DisplayName;
        BasePath = baseProjectPath;
        MenuItems = menuItems;
    }
    
    /// <summary>
    ///     What project this nav is for
    /// </summary>
    public int ProjectId { get; }
    
    /// <summary>
    ///     What project version this nav is for
    /// </summary>
    public int ProjectVersionId { get; }
    
    /// <summary>
    ///     Name of this project to display
    /// </summary>
    public string ProjectName { get; }
    
    /// <summary>
    ///     Base path of this project
    /// </summary>
    public string BasePath { get; }
    
    /// <summary>
    ///     All the menu items to display
    /// </summary>
    public IReadOnlyList<MenuItem> MenuItems { get; }
}