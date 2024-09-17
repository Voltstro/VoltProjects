namespace VoltProjects.Server.Models.Searching;

public class SearchResult
{
    public string Headline { get; set; }
    
    public string Path { get; init; }
    
    public string Title { get; init; }
    
    public string ProjectName { get; init; }
    
    public string ProjectDisplayName { get; init; }
    
    public string ProjectVersion { get; init; }
}