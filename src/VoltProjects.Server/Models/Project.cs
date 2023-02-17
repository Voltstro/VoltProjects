using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Server.Models;

/// <summary>
///     A project that we serve docs for
/// </summary>
[Table("Project")]
public class Project
{
    [Key]
    public int Id { get; init; }
    
    [Required]
    public string Name { get; set; }
    
    public string? ShortName { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    [Required]
    public string GitUrl { get; set; }

    public bool GitIsUrl => GitUrl.StartsWith("https://");
    
    [Required]
    public string GitBranch { get; set; }

    public string? IconPath { get; set; }
}