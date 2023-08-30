using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

/// <summary>
///     Application that builds docs
/// </summary>
[Table("doc_builder")]
public class DocBuilder
{
    /// <summary>
    ///     Generated primary key
    /// </summary>
    [Key]
    public string Id { get; init; }
    
    /// <summary>
    ///     Display name of this builder
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    ///     The name of the program it self
    /// </summary>
    public string Application { get; set; }
    
    /// <summary>
    ///     Arguments for the program
    /// </summary>
    [Column("arguments", TypeName = "text[]")]
    public string[]? Arguments { get; set; }
    
    /// <summary>
    ///     Environment variables to use
    /// </summary>
    [Column("environment_variables", TypeName = "text[]")]
    public string[]? EnvironmentVariables { get; set; }
}