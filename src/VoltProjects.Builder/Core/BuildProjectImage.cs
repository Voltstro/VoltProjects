using System.Security.Cryptography;
using VoltProjects.Builder.Services.Storage;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core;

/// <summary>
///     Info about image that is being built that may or may not need to be uploaded to cloud storage
/// </summary>
public class BuildProjectImage
{
    /// <summary>
    ///     Creates a new <see cref="BuildProjectImage"/> instance
    /// </summary>
    /// <param name="config"></param>
    /// <param name="page"></param>
    /// <param name="originalImagePath"></param>
    /// <param name="fullImagePathInProject"></param>
    public BuildProjectImage(VoltProjectsBuilderConfig config, ProjectPage page, string originalImagePath, string fullImagePathInProject)
    {
        OriginalImagePathInProject = originalImagePath;
        ProjectPage = page;
        
        FileStream = File.OpenRead(fullImagePathInProject);
        Hash = Helper.GetFileHash(FileStream);
        
        Uri baseImagePath = new(config.StorageConfig.PublicUrl);
        
        //Image contains path without URL
        ImagePath = Path.ChangeExtension(Path.Combine(page.ProjectVersion.Project.Name, page.ProjectVersion.VersionTag,
            OriginalImagePathInProject), ".webp");
        
        //Full image path contains URL
        string imagePath = new Uri(baseImagePath, ImagePath).ToString();
        imagePath = Path.ChangeExtension(imagePath, ".webp");
        FullImagePath = imagePath;
    }

    /// <summary>
    ///     Original unaltered image path
    /// </summary>
    public string OriginalImagePathInProject { get; }
    
    /// <summary>
    ///     Path to the image (Without URL)
    /// </summary>
    public string ImagePath { get; }
    
    /// <summary>
    ///     Full path to the image in the cloud storage (Including URL)
    /// </summary>
    public string FullImagePath { get; }
    
    /// <summary>
    ///     The original <see cref="ProjectPage"/> this image was found on
    /// </summary>
    public ProjectPage ProjectPage { get; }
    
    /// <summary>
    ///     Hash of the image
    /// </summary>
    public string Hash { get; }
    
    /// <summary>
    ///     <see cref="FileStream"/> to the image file
    /// </summary>
    public FileStream FileStream { get; }
    
    /// <summary>
    ///     Created <see cref="StorageItem"/> for this image
    /// </summary>
    public StorageItem? StorageItem { get; set; }
}