using VoltProjects.Builder.Services;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core.Building.ExternalObjects;

/// <summary>
///     Represents a "generic" object. Can be used for any file type
/// </summary>
public class GenericExternalObject : IExternalObjectHandler
{
    /// <summary>
    ///     <see cref="GenericExternalObject"/> that belongs to a <see cref="ProjectExternalItem"/>
    /// </summary>
    /// <param name="fullFilePath"></param>
    /// <param name="filePathRelativeToBuiltDocs"></param>
    /// <param name="externalItem"></param>
    public GenericExternalObject(string fullFilePath, string filePathRelativeToBuiltDocs,
        ProjectExternalItem externalItem)
        : this(fullFilePath, filePathRelativeToBuiltDocs, externalItem.ProjectVersion.Project.Name, externalItem.ProjectVersion.VersionTag)
    {
        ExternalItem = externalItem;
    }

    /// <summary>
    ///     Base constructor for a <see cref="GenericExternalObject"/>
    /// </summary>
    /// <param name="fullFilePath"></param>
    /// <param name="filePathRelativeToBuiltDocs"></param>
    /// <param name="projectName"></param>
    /// <param name="projectVersion"></param>
    public GenericExternalObject(string fullFilePath, string filePathRelativeToBuiltDocs, string projectName, string projectVersion)
    {
        PathInBuiltDocs = filePathRelativeToBuiltDocs;
        
        UploadPath = Path.Combine(projectName, projectVersion, filePathRelativeToBuiltDocs);
        
        ContentType = MimeMap.GetMimeType(filePathRelativeToBuiltDocs);
        
        ObjectStream = File.OpenRead(fullFilePath);
        Hash = Helper.GetFileHash(ObjectStream);
        ProjectPages = new List<ProjectPage>();
    }
    
    public string PathInBuiltDocs { get; }
    
    public string UploadPath { get; }

    public string ContentType { get; }
    
    public string Hash { get; }
    
    public virtual Task<Stream> GetUploadFileStream()
    {
        return Task.FromResult(ObjectStream);
    }

   

    /// <summary>
    ///     Underlining <see cref="Stream"/> to a file's data
    /// </summary>
    protected Stream ObjectStream { get; set; }

    #region Tracking
    
    public List<ProjectPage> ProjectPages { get; }
    
    public ProjectExternalItem? ExternalItem { get; }

    #endregion

    ~GenericExternalObject()
    {
        ReleaseResources();
    }
    
    public void Dispose()
    {
        ReleaseResources();
        GC.SuppressFinalize(this);
    }

    private void ReleaseResources()
    {
        ObjectStream.Dispose();
    }
}