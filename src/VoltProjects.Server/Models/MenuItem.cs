namespace VoltProjects.Server.Models;

public struct MenuItem
{
    public string Title { get; init; }
    
    public string Href { get; init; }
    
    public bool IsActive { get; init; }
}