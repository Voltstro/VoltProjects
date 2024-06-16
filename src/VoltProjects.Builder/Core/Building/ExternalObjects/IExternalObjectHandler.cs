using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core.Building.ExternalObjects;

/// <summary>
///     Base interface for an object that needs be uploaded
/// </summary>
public interface IExternalObjectHandler : IDisposable
{
    /// <summary>
    ///     Path of the object that is being uploaded
    ///
    ///     <para>Should be relative to the assigned built docs folder</para>
    /// </summary>
    public string PathInBuiltDocs { get; }
    
    /// <summary>
    ///     Path for the uploaded object
    /// </summary>
    public string UploadPath { get;  }
    
    /// <summary>
    ///     Content type of the file to upload
    /// </summary>
    public string ContentType { get; }
    
    /// <summary>
    ///     Hash of the file
    /// </summary>
    public string Hash { get; }

    /// <summary>
    ///     Returns a <see cref="Stream"/> that can be used to be uploaded
    /// </summary>
    /// <returns></returns>
    public Task<Stream> GetUploadFileStream();
    
    /// <summary>
    ///     <see cref="ProjectPage"/> that this object belongs to
    /// </summary>
    public List<ProjectPage> ProjectPages { get; }
    
    /// <summary>
    ///     <see cref="ProjectExternalItem"/> that this object belongs to
    /// </summary>
    public ProjectExternalItem? ExternalItem { get; }
}