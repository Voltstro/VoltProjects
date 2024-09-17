using System.Text.Json;
using Microsoft.Extensions.Options;
using VoltProjects.Server.Models;
using VoltProjects.Server.Shared;

namespace VoltProjects.Server.Services;

/// <summary>
///     Service for website's structured data
/// </summary>
public sealed class StructuredDataService
{
    public StructuredDataService(IOptions<VoltProjectsConfig> config)
    {
        VoltProjectsConfig configValue = config.Value;

        StructuredData = new StructuredData("WebSite", "Volt Projects", configValue.SiteUrl);
        StructuredDataJson = JsonSerializer.Serialize(StructuredData, new JsonSerializerOptions { WriteIndented = true });
    }
    
    public StructuredData StructuredData { get; }
    
    public string StructuredDataJson { get; }
}