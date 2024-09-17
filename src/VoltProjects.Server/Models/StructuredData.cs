using System.Text.Json.Serialization;

namespace VoltProjects.Server.Models;

public struct StructuredData
{
    public StructuredData(string type,  string name, string url)
    {
        Type = type;
        Name = name;
        Url = url;
    }
    
    [JsonPropertyName("@context")]
    public string Context { get; } = "https://schema.org";
    
    [JsonPropertyName("@type")]
    public string Type { get; }
    
    [JsonPropertyName("name")]
    public string Name { get; }
    
    [JsonPropertyName("url")]
    public string Url { get; }
}