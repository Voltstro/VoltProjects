using System.Collections.Generic;

namespace VoltProjects.Server.Models;

public class TocItem
{
    public int Id { get; init; }
    
    public string? Title { get; init; }
    
    public string? Href { get; init; }
    
    public bool IsActive { get; set; }
    
    public List<TocItem>? Items { get; set; }
}