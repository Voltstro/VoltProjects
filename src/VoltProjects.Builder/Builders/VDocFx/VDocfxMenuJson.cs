using System.Text.Json.Serialization;

namespace VoltProjects.Builder.Builders.VDocFx;

public struct VDocfxMenuJson
{
    [JsonPropertyName("items")]
    public VDocfxMenuItem[] Items { get; init; }
    
    public struct VDocfxMenuItem
    {
        [JsonPropertyName("href")]
        public string Href { get; init; }
        
        [JsonPropertyName("name")]
        public string Name { get; init; }
    }
}