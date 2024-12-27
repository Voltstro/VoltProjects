using Microsoft.Extensions.DependencyInjection;
using VoltProjects.Builder.Core;

namespace VoltProjects.Builder.Services.Storage;

internal static class StorageServiceInstaller
{
    public static void InstallStorageServiceProvider(this IServiceCollection serviceCollection, VoltProjectsBuilderConfig config)
    {
        StorageConfig? storageConfig = config.ObjectStorageProvider;
        if (storageConfig == null)
            throw new NullReferenceException("Storage Config is not set!");

        if (string.IsNullOrWhiteSpace(storageConfig.ContainerName) || string.IsNullOrWhiteSpace(storageConfig.BasePath))
            throw new NullReferenceException("Both ContainerName and BasePath cannot be null or white space!");

        if (!string.IsNullOrWhiteSpace(storageConfig.SubPath) && !storageConfig.SubPath.EndsWith('/'))
            throw new ArgumentException("SubPath should end with a '/'!");
        
        switch(storageConfig.Provider)
        {
            case StorageProvider.Azure:
                serviceCollection.AddSingleton<IStorageService>(new AzureStorageService(storageConfig));
                break;
            case StorageProvider.Google:
                serviceCollection.AddSingleton<IStorageService>(new GoogleStorageService(storageConfig));
                break;
            case StorageProvider.S3:
                serviceCollection.AddSingleton<IStorageService>(new S3StorageService(storageConfig));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}