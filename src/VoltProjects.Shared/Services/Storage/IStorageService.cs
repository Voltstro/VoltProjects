namespace VoltProjects.Shared.Services.Storage;

/// <summary>
///     Interface for a storage service
/// </summary>
public interface IStorageService
{
    /// <summary>
    ///     Bulk upload a bunch of different files
    /// </summary>
    /// <param name="filesToUpload"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task UploadBulkFileAsync<TUploadFile>(TUploadFile[] filesToUpload, CancellationToken cancellationToken = default) where TUploadFile : IUploadFile;

    /// <summary>
    ///     Uploads a file
    /// </summary>
    /// <param name="fileUpload"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TUploadFile"></typeparam>
    /// <returns></returns>
    public Task UploadFileAsync<TUploadFile>(TUploadFile fileUpload, CancellationToken cancellationToken = default)
        where TUploadFile : IUploadFile;

    /// <summary>
    ///     Gets the full upload URL for a <see cref="TUploadFile"/>
    /// </summary>
    /// <param name="externalObject"></param>
    /// <returns></returns>
    public string GetFullUploadUrl<TUploadFile>(TUploadFile externalObject) where TUploadFile : IUploadFile;
}