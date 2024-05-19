using VoltProjects.Builder.Core.Building.ExternalObjects;

namespace VoltProjects.Builder.Services.Storage;

/// <summary>
///     Interface for a storage service
/// </summary>
public interface IStorageService
{
    //public Task<uint?> GetFileHashAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads a file to the storage provider
    /// </summary>
    /// <param name="fileStream"></param>
    /// <param name="fileName"></param>
    /// <param name="contentType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType,
        CancellationToken cancellationToken = default);

    public Task UploadBulkFileAsync(IExternalObjectHandler[] filesToUpload, CancellationToken cancellationToken = default);

    public string GetFullUploadUrl(IExternalObjectHandler externalObject);
}