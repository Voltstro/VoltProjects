using System.Text.Json.Serialization;

namespace VoltProjects.Builder.VDocFx;

public class VDocFxPageModel
{
    [JsonPropertyName("content")]
    public string Content { get; init; }
    
    [JsonPropertyName("rawMetadata")]
    public Metadata Metadata { get; init; }
}

public struct Metadata
{
    [JsonPropertyName("title")] public string? Title { get; init; }

    [JsonPropertyName("rawTitle")] public string? RawTitle { get; init; }

    [JsonPropertyName("wordCount")] public int? WordCount { get; init; }

    [JsonPropertyName("page_type")] public string? PageType { get; init; }

    [JsonPropertyName("layout")] public string Layout { get; init; }

    [JsonPropertyName("original_content_git_url")]
    public string? GitUrl { get; init; }

    [JsonPropertyName("_tocRel")] public string? TocRel { get; init; }

    [JsonPropertyName("_op_gitContributorInformation")]
    public GitContributorInfo ContributorInfo { get; init; }

    public struct GitContributorInfo
    {
        [JsonPropertyName("updated_at_date_time")]
        public DateTime UpdateDate { get; init; }

        [JsonPropertyName("contributors")] public GitContributor[]? Contributors { get; init; }
    }

    public struct GitContributor
    {
        [JsonPropertyName("name")] public string Name { get; init; }

        [JsonPropertyName("id")] public string Id { get; init; }
    }
}