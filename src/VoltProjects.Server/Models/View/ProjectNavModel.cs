namespace VoltProjects.Server.Models.View;

/// <summary>
///     View Model for ProjectNav
/// </summary>
public sealed class ProjectNavModel
{
    /// <summary>
    ///     Name of this project to display
    /// </summary>
    public string ProjectName { get; init; }
    
    /// <summary>
    ///     Base path of this project
    /// </summary>
    public string BasePath { get; init; }
    
    /// <summary>
    ///     All the menu items to display
    /// </summary>
    public MenuItem[] MenuItems { get; init; }
    
    /// <summary>
    ///     The project's git URL
    /// </summary>
    public string GitUrl { get; init; }
}