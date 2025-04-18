using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

/// <summary>
///     A project that we serve docs for
/// </summary>
[Table("project")]
public class Project
{
    /// <summary>
    ///     Project primary key. Generated.
    /// </summary>
    [Key]
    public int Id { get; init; }
    
    /// <summary>
    ///     Name of this project
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Display name of this project
    /// </summary>
    public string DisplayName { get; set; }
    
    /// <summary>
    ///     Short name of this project
    /// </summary>
    public string? ShortName { get; set; }
    
    /// <summary>
    ///     Brief description of the project
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     The URL or path to the git repo
    /// </summary>
    public string GitUrl { get; set; } = "https://github.com/";

    /// <summary>
    ///     Is <see cref="GitUrl"/> a url or path
    /// </summary>
    public bool GitIsUrl => GitUrl.StartsWith("https://");

    /// <summary>
    ///     Path to the icon
    /// </summary>
    public string? IconPath { get; set; }

    /// <summary>
    ///     Is this project published
    /// </summary>
    public bool Published { get; set; } = true;
    
    /// <summary>
    ///     When was the last time this project was updated
    /// </summary>
    public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    ///     When was this project created?
    /// </summary>
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;

    public ICollection<ProjectVersion> ProjectVersions { get; } = new List<ProjectVersion>();
}