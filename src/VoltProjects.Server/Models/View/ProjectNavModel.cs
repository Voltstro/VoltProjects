namespace VoltProjects.Server.Models.View;

public class ProjectNavModel
{
    public string ProjectName { get; init; }
    
    public string BasePath { get; init; }
    
    public MenuItem[] MenuItems { get; init; }
}