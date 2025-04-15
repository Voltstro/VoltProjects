namespace VoltProjects.Shared.Services.Storage;

public class GenericUploadFile : IUploadFile
{
    private readonly Stream stream;
    
    public GenericUploadFile(Stream stream, string contentType, string uploadPath)
    {
        this.stream = stream;
        ContentType = contentType;
        UploadPath = uploadPath;
    }
    
    public string UploadPath { get; }
    
    public string ContentType { get; }
    
    public Task<Stream> GetUploadFileStream()
    {
        return Task.FromResult(stream);
    }
}