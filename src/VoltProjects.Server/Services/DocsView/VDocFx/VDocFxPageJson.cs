using System;
using System.Text.Json.Serialization;

namespace VoltProjects.Server.Services.DocsView.VDocFx;

public class VDocFxPageJson
{
    [JsonPropertyName("content")]
    public string Content { get; init; }
    
    [JsonPropertyName("rawMetadata")]
    public Metadata Metadata { get; init; }
}

public struct Metadata
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }
    
    [JsonPropertyName("rawTitle")]
    public string? RawTitle { get; init; }
    
    [JsonPropertyName("wordCount")]
    public int? WordCount { get; init; }
    
    [JsonPropertyName("page_type")]
    public string? PageType { get; init; }
    
    [JsonPropertyName("layout")]
    public string Layout { get; init; }
    
    [JsonPropertyName("original_content_git_url")]
    public string? GitUrl { get; init; }
    
    //[JsonPropertyName("updated_at")]
    //public DateTime? UpdatedAt { get; init; }
}