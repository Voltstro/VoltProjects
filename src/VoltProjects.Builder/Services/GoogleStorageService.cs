using Google;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using VoltProjects.Builder.Core;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace VoltProjects.Builder.Services;

/// <summary>
///     Provides API access to Google Cloud Storage
/// </summary>
public sealed class GoogleStorageService
{
    private readonly StorageConfig config;
    private readonly StorageClient storageClient;
    
    /// <summary>
    ///     Creates a new <see cref="GoogleStorageService"/>
    /// </summary>
    /// <param name="config"></param>
    /// <exception cref="FileNotFoundException"></exception>
    public GoogleStorageService(IOptions<VoltProjectsBuilderConfig> config)
    {
        this.config = config.Value.StorageConfig;

        string? googleCredentialLocation = Environment.GetEnvironmentVariable("VP_GOOGLE_CREDENTIAL");
        if (string.IsNullOrWhiteSpace(googleCredentialLocation))
            throw new FileNotFoundException("Google credential was not provided!");

        GoogleCredential? credential = GoogleCredential.FromFile(googleCredentialLocation);
        storageClient = StorageClient.Create(credential);
    }

    /// <summary>
    ///     Gets a file's CRC32C hash
    ///     <para>Null if the file is not found</para>
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public async Task<uint?> GetFileHashAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            Object? foundObject = await storageClient.GetObjectAsync(config.BucketName, filePath, cancellationToken: cancellationToken);
            if (foundObject == null)
                return null;

            byte[] data = Convert.FromBase64String(foundObject.Crc32c);
            return BitConverter.ToUInt32(data);
        }
        catch (GoogleApiException ex)
        {
            //Not found, that fine
            if (ex.Error.Code == 404)
                return null;

            throw;
        }
    }

    /// <summary>
    ///     Uploads a file
    /// </summary>
    /// <param name="fileStream"></param>
    /// <param name="fileName"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        Object? result = await storageClient.UploadObjectAsync(config.BucketName, fileName, contentType, fileStream, cancellationToken: cancellationToken);
        return Path.Combine(config.PublicUrl, fileName);
    }
}