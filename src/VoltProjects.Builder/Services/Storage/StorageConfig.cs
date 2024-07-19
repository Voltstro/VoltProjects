namespace VoltProjects.Builder.Services.Storage;

/// <summary>
///     Config related to storage
/// </summary>
public sealed class StorageConfig
{
    /// <summary>
    ///     Storage provider to use
    /// </summary>
    public StorageProvider Provider { get; init; } = StorageProvider.Azure;
    
    /// <summary>
    ///     Name of the container to upload to
    /// </summary>
    public string? ContainerName { get; set; }
    
    /// <summary>
    ///     The base public URL
    /// </summary>
    public string? BasePath { get; set; }
    
    /// <summary>
    ///     Additional sub path to add to <see cref="BasePath"/>
    /// </summary>
    public string? SubPath { get; set; }
}