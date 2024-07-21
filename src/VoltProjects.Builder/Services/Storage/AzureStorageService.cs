using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using VoltProjects.Builder.Core.Building.ExternalObjects;

namespace VoltProjects.Builder.Services.Storage;

/// <summary>
///     <see cref="IStorageService"/> that uses Azure Storage
/// </summary>
internal sealed class AzureStorageService : IStorageService
{
    private readonly StorageConfig config;
    private readonly BlobContainerClient storageClient;
    
    /// <summary>
    ///     Creates a new <see cref="AzureStorageService"/> instance
    /// </summary>
    /// <param name="config"></param>
    /// <exception cref="FileNotFoundException"></exception>
    public AzureStorageService(StorageConfig config)
    {
        this.config = config;

        string? azureCredentialString = Environment.GetEnvironmentVariable("VP_AZURE_CREDENTIAL");
        if (string.IsNullOrWhiteSpace(azureCredentialString))
            throw new FileNotFoundException("Azure credential was not provided!");

        BlobServiceClient blobServiceClient = new(azureCredentialString);
        storageClient = blobServiceClient.GetBlobContainerClient(this.config.ContainerName);
    }

    public async Task UploadBulkFileAsync(IExternalObjectHandler[] filesToUpload, CancellationToken cancellationToken = default)
    {
        IEnumerable<GroupedStorageItem> groupedStorageItems = filesToUpload.GroupBy(x => x.ContentType, item => item,
            (s, items) => new GroupedStorageItem(s, items.ToArray()));
        BlobHttpHeaders blobHttpHeader = new()
        {
            CacheControl = $"public,max-age={config.CacheTime}"
        };
        BlobUploadOptions options = new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeader,
            TransferOptions = new StorageTransferOptions
            {
                // Set the maximum number of workers that 
                // may be used in a parallel transfer.
                MaximumConcurrency = 8,

                // Set the maximum length of a transfer to 50MB.
                MaximumTransferSize = 50 * 1024 * 1024
            }
        };

        foreach (GroupedStorageItem groupedStorageItem in groupedStorageItems)
        {
            Queue<Task<Response<BlobContentInfo>>> tasks = new();

            blobHttpHeader.ContentType = groupedStorageItem.ContentType;
            foreach (IExternalObjectHandler storageItem in groupedStorageItem.Items)
            {
                string uploadPath = Path.Combine(config.SubPath!, storageItem.UploadPath);
                BlobClient newBlob = storageClient.GetBlobClient(uploadPath);
                Stream fileStream = await storageItem.GetUploadFileStream();
                tasks.Enqueue(newBlob.UploadAsync(fileStream, options, cancellationToken));
            }

            await Task.WhenAll(tasks);
        }
    }

    public string GetFullUploadUrl(IExternalObjectHandler externalObject)
    {
        Uri baseUri = new(config.BasePath!);
        Uri fullUri = new(baseUri, Path.Combine(config.ContainerName!, config.SubPath ?? string.Empty, externalObject.UploadPath));
        return fullUri.ToString();
    }

    private record GroupedStorageItem(string ContentType, IExternalObjectHandler[] Items);
}