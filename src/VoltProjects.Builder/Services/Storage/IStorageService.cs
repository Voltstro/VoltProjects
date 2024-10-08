using VoltProjects.Builder.Core.Building.ExternalObjects;

namespace VoltProjects.Builder.Services.Storage;

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
    public Task UploadBulkFileAsync(IExternalObjectHandler[] filesToUpload, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the full upload URL for a <see cref="IExternalObjectHandler"/>
    /// </summary>
    /// <param name="externalObject"></param>
    /// <returns></returns>
    public string GetFullUploadUrl(IExternalObjectHandler externalObject);
}