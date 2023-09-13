namespace VoltProjects.Builder.Services.Storage;

public struct StorageItem
{
    public Stream ItemStream { get; set; }
    
    public string FileName { get; set; }
    
    public string OriginalFilePath { get; set; }
    
    public string Hash { get; set; }
}