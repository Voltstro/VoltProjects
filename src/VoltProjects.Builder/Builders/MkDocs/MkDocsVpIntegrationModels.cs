using System.Text.Json.Serialization;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Builders.MkDocs;

public class MkDocsVpIntegrationModels
{
    public struct PagesModel
    {
        [JsonPropertyName("pages")]
        public Page[] Pages { get; set; }
    }
    
    public struct Page
    {
        [JsonPropertyName("path")]
        public string Path { get; set; }
        
        [JsonPropertyName("content")]
        public string Content { get; set; }
        
        [JsonPropertyName("title")]
        public string Title { get; set; }
        
        [JsonPropertyName("file_path")]
        public string FilePath { get; set; }
        
        [JsonPropertyName("toc_index")]
        public string? TocIndex { get; set; }
    }
    
    public struct MenuModel
    {
        [JsonPropertyName("menu")]
        public LinkItem[] MenuItems { get; set; }
    }
    
    public struct TocModel
    {
        [JsonPropertyName("tocs")]
        public TocItem[] Tocs { get; set; }
    }
    
    public struct TocItem
    {
        [JsonPropertyName("toc_index")]
        public string TocIndex { get; set; }
        
        [JsonPropertyName("toc_items")]
        public LinkItem[] LinkItems { get; set; }
    }
}