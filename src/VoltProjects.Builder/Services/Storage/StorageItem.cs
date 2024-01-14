namespace VoltProjects.Builder.Services.Storage;

/// <summary>
///     An item that needs to be uploaded to cloud storage
/// </summary>
public struct StorageItem
{
    /// <summary>
    ///     <see cref="Stream"/> containing the item's data
    /// </summary>
    public Stream ItemStream { get; set; }
    
    /// <summary>
    ///     File name / Path of the item
    /// </summary>
    public string FileName { get; set; }
    
    /// <summary>
    ///     Content type of the item
    /// </summary>
    public string ContentType { get; set; }
    
    /// <summary>
    ///     Original file path of the item
    /// </summary>
    public string OriginalFilePath { get; set; }
    
    /// <summary>
    ///     Hash of the item
    /// </summary>
    public string Hash { get; set; }
}