using Amazon.S3;
using Amazon.S3.Model;

namespace VoltProjects.Shared.Services.Storage;

internal sealed class S3StorageService : IStorageService
{
    private readonly StorageConfig config;
    private readonly AmazonS3Client client;
    
    public S3StorageService(StorageConfig config)
    {
        this.config = config;
        
        string? s3KeyId = Environment.GetEnvironmentVariable("VP_S3_KEY_ID");
        string? s3AccessKey = Environment.GetEnvironmentVariable("VP_S3_ACCESS_KEY");
        string? s3Service = Environment.GetEnvironmentVariable("VP_S3_SERVICE_URL");
        
        if (string.IsNullOrWhiteSpace(s3KeyId) || string.IsNullOrWhiteSpace(s3AccessKey) || string.IsNullOrWhiteSpace(s3Service))
            throw new FileNotFoundException("S3 details were not provided! Require VP_S3_KEY_ID, VP_S3_ACCESS_KEY and VP_S3_SERVICE_URL.");

        AmazonS3Config s3Config = new()
        {
            ServiceURL = s3Service,
            ForcePathStyle = true
        };
        
        client = new AmazonS3Client(s3KeyId, s3AccessKey, s3Config);
    }

    public async Task UploadBulkFileAsync<TUploadFile>(TUploadFile[] filesToUpload, CancellationToken cancellationToken = default) where TUploadFile : IUploadFile
    {
        Queue<Task> tasks = new();

        foreach (TUploadFile fileUpload in filesToUpload)
        {
            tasks.Enqueue(UploadFileAsync(fileUpload, cancellationToken));
        }
        
        await Task.WhenAll(tasks);
    }

    public async Task UploadFileAsync<TUploadFile>(TUploadFile fileUpload, CancellationToken cancellationToken = default) where TUploadFile : IUploadFile
    {
        Stream fileStream = await fileUpload.GetUploadFileStream();
        string uploadPath = Path.Combine(config.SubPath!, fileUpload.UploadPath);
            
        PutObjectRequest request = new()
        {
            BucketName = config.ContainerName,
            Key = uploadPath,
            InputStream = fileStream,
            DisablePayloadSigning = true,
            ContentType = fileUpload.ContentType,
            AutoCloseStream = false,
            Headers =
            {
                CacheControl = $"public,max-age={config.CacheTime}"
            }
        };

        await client.PutObjectAsync(request, cancellationToken);
    }

    public string GetFullUploadUrl<TUploadFile>(TUploadFile externalObject) where TUploadFile : IUploadFile
    {
        Uri baseUri = new(config.BasePath!);
        Uri fullUri = new(baseUri, Path.Combine(config.SubPath ?? string.Empty, externalObject.UploadPath));
        return fullUri.ToString();
    }
}