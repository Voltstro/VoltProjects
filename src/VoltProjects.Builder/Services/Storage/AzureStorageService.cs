using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Options;
using VoltProjects.Builder.Core;

namespace VoltProjects.Builder.Services.Storage;

public sealed class AzureStorageService : IStorageService
{
    private readonly StorageConfig config;
    private readonly BlobContainerClient storageClient;
    
    /// <summary>
    ///     Creates a new <see cref="AzureStorageService"/>
    /// </summary>
    /// <param name="config"></param>
    /// <exception cref="FileNotFoundException"></exception>
    public AzureStorageService(IOptions<VoltProjectsBuilderConfig> config)
    {
        this.config = config.Value.StorageConfig;

        string? azureCredentialString = Environment.GetEnvironmentVariable("VP_AZURE_CREDENTIAL");
        if (string.IsNullOrWhiteSpace(azureCredentialString))
            throw new FileNotFoundException("Azure credential was not provided!");

        BlobServiceClient blobServiceClient = new(azureCredentialString);
        storageClient = blobServiceClient.GetBlobContainerClient(this.config.ContainerName);
    }
    
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType,
        CancellationToken cancellationToken = default)
    {
        BlobClient? newBlob = storageClient.GetBlobClient(fileName);
        BlobHttpHeaders blobHttpHeader = new() { ContentType = contentType };
        
        await newBlob.UploadAsync(fileStream, new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeader
        }, cancellationToken);
        
        return Path.Combine(config.PublicUrl, fileName);
    }

    public async Task UploadBulkFileAsync(StorageItem[] filesToUpload, string contentType, CancellationToken cancellationToken = default)
    {
        BlobHttpHeaders blobHttpHeader = new() { ContentType = contentType };
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
        
        Queue<Task<Response<BlobContentInfo>>> tasks = new();

        foreach (StorageItem storageItem in filesToUpload)
        {
            BlobClient newBlob = storageClient.GetBlobClient(storageItem.FileName);
            tasks.Enqueue(newBlob.UploadAsync(storageItem.ItemStream, options, cancellationToken));
        }

        await Task.WhenAll(tasks);
    }
}