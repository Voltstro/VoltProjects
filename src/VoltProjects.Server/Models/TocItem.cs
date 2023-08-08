namespace VoltProjects.Server.Models;

public struct TocItem
{
    public string? Title { get; init; }
    
    public string? Href { get; init; }
    
    public bool IsActive { get; init; }
    
    public TocItem[]? Items { get; init; }
}