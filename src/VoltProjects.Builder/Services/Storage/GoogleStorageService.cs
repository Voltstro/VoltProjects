using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using VoltProjects.Builder.Core.Building.ExternalObjects;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace VoltProjects.Builder.Services.Storage;

internal sealed class GoogleStorageService : IStorageService
{
    private readonly StorageConfig config;
    private readonly StorageClient storageClient;

    public GoogleStorageService(StorageConfig config)
    {
        this.config = config;
        
        string? gcsCredentialString = Environment.GetEnvironmentVariable("VP_GCS_CREDENTIAL");
        if (string.IsNullOrWhiteSpace(gcsCredentialString))
            throw new FileNotFoundException("Google credential was not provided!");

        GoogleCredential credentials = GoogleCredential.FromJson(gcsCredentialString);
        storageClient = StorageClient.Create(credentials);
    }

    public async Task UploadBulkFileAsync(IExternalObjectHandler[] filesToUpload, CancellationToken cancellationToken = default)
    {
        Queue<Task<Object>> tasks = new();
        
        foreach (IExternalObjectHandler storageItem in filesToUpload)
        {
            Stream fileStream = await storageItem.GetUploadFileStream();
            string uploadPath = Path.Combine(config.SubPath!, storageItem.UploadPath);

            Object storageObject = new()
            {
                Bucket = config.ContainerName,
                Name = uploadPath,
                CacheControl = $"public,max-age={config.CacheTime}",
                ContentType = storageItem.ContentType
            };
            
            tasks.Enqueue(storageClient.UploadObjectAsync(storageObject, fileStream, cancellationToken: cancellationToken));
        }
        
        await Task.WhenAll(tasks);
    }

    public string GetFullUploadUrl(IExternalObjectHandler externalObject)
    {
        string baseUrl = $"https://{Path.Combine(config.BasePath!, config.SubPath ?? string.Empty)}";
        
        Uri baseUri = new(baseUrl);
        Uri fullUri = new(baseUri, externalObject.UploadPath);
        return fullUri.ToString();
    }
}
