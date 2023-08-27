using Google;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using VoltProjects.Builder.Core;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace VoltProjects.Builder.Services;

public sealed class GoogleStorageService
{
    private readonly StorageConfig config;
    private readonly StorageClient storageClient;
    
    public GoogleStorageService(IOptions<VoltProjectsBuilderConfig> config)
    {
        this.config = config.Value.StorageConfig;

        string? googleCredentialLocation = Environment.GetEnvironmentVariable("VP_GOOGLE_CREDENTIAL");
        if (string.IsNullOrWhiteSpace(googleCredentialLocation))
            throw new FileNotFoundException("Google credential was not provided!");

        GoogleCredential? credential = GoogleCredential.FromFile(googleCredentialLocation);
        storageClient = StorageClient.Create(credential);
    }

    public async Task<uint?> GetFileHashAsync(string filePath)
    {
        try
        {
            Object? foundObject = await storageClient.GetObjectAsync(config.BucketName, filePath);
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

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        Object? result = await storageClient.UploadObjectAsync(config.BucketName, fileName, contentType, fileStream);
        return Path.Combine(config.PublicUrl, fileName);
    }
}