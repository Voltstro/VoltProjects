using System.Text.Json.Serialization;

namespace VoltProjects.Builder.Builders.VDocFx;

public struct VDocFxTocJson
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }
    
    [JsonPropertyName("href")]
    public string? Href { get; init; }
    
    [JsonPropertyName("items")]
    public VDocFxTocJson[]? Items { get; init; }
}