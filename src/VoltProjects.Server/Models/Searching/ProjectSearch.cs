namespace VoltProjects.Server.Models.Searching;

public sealed class ProjectSearch
{
    public ProjectSearch(int id, string displayName, ProjectVersionSearch[] projectVersions)
    {
        Id = id;
        DisplayName = displayName;
        Versions = projectVersions;
    }
    
    public int Id { get; set; }
    
    public string DisplayName { get; set; }
    
    public bool Active { get; set; }
    
    public ProjectVersionSearch[] Versions { get; set; }
}
