using System.Text.Json.Serialization;

namespace VoltProjects.Builder.Builders.DocFx;

public class DocFxRawModel
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("conceptual")]
    public string? Conceptual { get; set; }
    
    [JsonPropertyName("uid")]
    public string? Uid { get; set; }
    
    [JsonPropertyName("wordCount")]
    public int WordCount { get; set; }
    
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("items")]
    public DocfxMenuItem[] Items { get; set; }
    
    [JsonPropertyName("_tocPath")]
    public string? TocPath { get; set; }
    
    [JsonPropertyName("_tocRel")]
    public string? TocRel { get; set; }
    
    [JsonPropertyName("_path")]
    public string Path { get; set; }
        
    public struct DocfxMenuItem
    {
        [JsonPropertyName("href")]
        public string? Href { get; init; }
        
        [JsonPropertyName("name")]
        public string? Name { get; init; }
        
        [JsonPropertyName("items")]
        public DocfxMenuItem[] Items { get; init; }
    }
}