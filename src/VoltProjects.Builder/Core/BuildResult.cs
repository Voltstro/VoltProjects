using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core;

/// <summary>
///     Should contain a Project's built menu, TOCs and pages
/// </summary>
public struct BuildResult
{
    /// <summary>
    ///     Creates a new <see cref="BuildResult"/> instance
    /// </summary>
    /// <param name="projectMenu"></param>
    /// <param name="tocs"></param>
    /// <param name="pages"></param>
    public BuildResult(ProjectMenuItem[] projectMenuItems, ProjectToc[] tocs, ProjectTocItem[] projectTocItems, ProjectPage[] pages)
    {
        ProjectMenuItems = projectMenuItems;
        ProjectTocs = tocs;
        ProjectTocItems = projectTocItems;
        ProjectPages = pages;
    }
    
    public ProjectMenuItem[] ProjectMenuItems { get; }
    
    public ProjectToc[] ProjectTocs { get; }
    
    public ProjectTocItem[] ProjectTocItems { get; }
    
    public ProjectPage[] ProjectPages { get; }
}