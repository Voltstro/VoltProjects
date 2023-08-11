using VoltProjects.Shared.Models;

namespace VoltProjects.Builder;

public struct BuildResult
{
    public BuildResult(ProjectMenu projectMenu, ProjectToc[] tocs, ProjectPage[] pages)
    {
        this.ProjectMenu = projectMenu;
        ProjectTocs = tocs;
        ProjectPages = pages;
    }
    
    public ProjectMenu ProjectMenu { get; init; }
    
    public ProjectToc[] ProjectTocs { get; init; }
    
    public ProjectPage[] ProjectPages { get; init; }
}