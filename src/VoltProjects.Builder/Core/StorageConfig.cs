namespace VoltProjects.Builder.Core;

/// <summary>
///     Config related to storage
/// </summary>
public sealed class StorageConfig
{
    /// <summary>
    ///     Name of the container to upload assets to
    /// </summary>
    public string ContainerName { get; set; }
    
    /// <summary>
    ///     What is the public base URL of the bucket
    /// </summary>
    public string PublicUrl { get; set; }
}