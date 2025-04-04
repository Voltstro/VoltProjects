using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

[Table("language")]
public class Language
{
    [Key]
    public int Id { get; init; }
    
    /// <summary>
    ///     General name of the language
    /// </summary>
    public string Name { get; init; }
    
    /// <summary>
    ///     Postgres language configuration
    /// </summary>
    [Column(TypeName = "oid")]
    public uint Configuration { get; init; }
}