using VoltProjects.Shared.Models;
using VoltProjects.Shared.Services.Storage;

namespace VoltProjects.Builder.Core.Building.ExternalObjects;

/// <summary>
///     Base interface for an object that needs be uploaded
/// </summary>
public interface IExternalObjectHandler : IUploadFile, IDisposable
{
    /// <summary>
    ///     Path of the object that is being uploaded
    ///
    ///     <para>Should be relative to the assigned built docs folder</para>
    /// </summary>
    public string PathInBuiltDocs { get; }
    
    /// <summary>
    ///     Hash of the file
    /// </summary>
    public string Hash { get; }
    
    /// <summary>
    ///     <see cref="ProjectPage"/> that this object belongs to
    /// </summary>
    public List<ProjectPage> ProjectPages { get; }
    
    /// <summary>
    ///     <see cref="ProjectExternalItem"/> that this object belongs to
    /// </summary>
    public ProjectExternalItem? ExternalItem { get; }
}