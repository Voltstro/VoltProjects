using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Models.View;

/// <summary>
///     View model for the main page
/// </summary>
public class MainViewModel
{
    public MainViewModel(Project[] projects, string publicUrl)
    {
        Projects = projects;
        PublicUrl = publicUrl;
    }
    
    /// <summary>
    ///     All projects
    /// </summary>
    public Project[] Projects { get; }
    
    /// <summary>
    ///     Public URL for assets
    /// </summary>
    public string PublicUrl { get; }
}