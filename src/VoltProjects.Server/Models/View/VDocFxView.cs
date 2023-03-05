namespace VoltProjects.Server.Models.View;

public class VDocFxViewModel
{
    public string ProjectName { get; init; }
    public string ProjectBasePath { get; init; }
    
    public string Content { get; set; }
    
    public string? Tile { get; set; }
    
    public string? RawTitle { get; set; }
    
    public string Layout { get; set; }
    
    public string PageType { get; set; }
    
    public string? GitUrl { get; set; }

    public MenuItem[]? MenuItems { get; set; }
    
    public VDocFxViewModel.TocItem? Toc { get; set; }
    
    public record MenuItem(string Name, string Href);

    public record TocItem(string? Name, string? Href, bool IsActive, TocItem[]? TocItems);
}