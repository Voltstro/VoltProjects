using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

[Table("Language")]
public class Language
{
    [Key]
    public int Id { get; init; }
    
    public string Name { get; init; }
}