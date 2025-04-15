namespace VoltProjects.Shared.Services.Storage;

public interface IUploadFile
{
    /// <summary>
    ///     Path for the uploaded object
    /// </summary>
    public string UploadPath { get;  }
    
    /// <summary>
    ///     Content type of the file to upload
    /// </summary>
    public string ContentType { get; }
    
    /// <summary>
    ///     Returns a <see cref="Stream"/> that can be used to be uploaded
    /// </summary>
    /// <returns></returns>
    public Task<Stream> GetUploadFileStream();
}