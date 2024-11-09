using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace VoltProjects.Shared.Services.Storage;

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

    public async Task UploadBulkFileAsync<TUploadFile>(TUploadFile[] filesToUpload, CancellationToken cancellationToken = default) where TUploadFile : IUploadFile
    {
        Queue<Task<Object>> tasks = new();
        
        foreach (TUploadFile storageItem in filesToUpload)
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

    public async Task UploadFileAsync<TUploadFile>(TUploadFile fileUpload, CancellationToken cancellationToken = default) where TUploadFile : IUploadFile
    {
        Stream fileStream = await fileUpload.GetUploadFileStream();
        string uploadPath = Path.Combine(config.SubPath!, fileUpload.UploadPath);

        Object storageObject = new()
        {
            Bucket = config.ContainerName,
            Name = uploadPath,
            CacheControl = $"public,max-age={config.CacheTime}",
            ContentType = fileUpload.ContentType
        };

        await storageClient.UploadObjectAsync(storageObject, fileStream, cancellationToken: cancellationToken);
    }

    public string GetFullUploadUrl<TUploadFile>(TUploadFile externalObject) where TUploadFile : IUploadFile
    {
        Uri baseUri = new(config.BasePath!);
        Uri fullUri = new(baseUri, Path.Combine(config.SubPath ?? string.Empty, externalObject.UploadPath));
        return fullUri.ToString();
    }
}
