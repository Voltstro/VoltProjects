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
    
    public string Name { get; set; }
    
    public string? ShortName { get; set; }
    
    public string Description { get; set; }

    public string GitUrl { get; set; } = "https://github.com/";

    public bool GitIsUrl => GitUrl.StartsWith("https://");

    public string? IconPath { get; set; } = "icon.svg";
}