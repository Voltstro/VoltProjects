using Microsoft.Extensions.DependencyInjection;

namespace VoltProjects.Shared.Services.Storage;

public static class StorageServiceInstaller
{
    public static void InstallStorageServiceProvider(this IServiceCollection serviceCollection, StorageConfig? config)
    {
        if (config == null)
            throw new NullReferenceException("Storage Config is not set!");

        if (string.IsNullOrWhiteSpace(config.ContainerName) || string.IsNullOrWhiteSpace(config.BasePath))
            throw new NullReferenceException("Both ContainerName and BasePath cannot be null or white space!");

        if (!string.IsNullOrWhiteSpace(config.SubPath) && !config.SubPath.EndsWith('/'))
            throw new ArgumentException("SubPath should end with a '/'!");
        
        switch(config.Provider)
        {
            case StorageProvider.Azure:
                serviceCollection.AddSingleton<IStorageService>(new AzureStorageService(config));
                break;
            case StorageProvider.Google:
                serviceCollection.AddSingleton<IStorageService>(new GoogleStorageService(config));
                break;
            case StorageProvider.S3:
                serviceCollection.AddSingleton<IStorageService>(new S3StorageService(config));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}