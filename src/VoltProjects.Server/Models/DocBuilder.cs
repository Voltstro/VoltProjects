using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Server.Models;

[Table("DocBuilder")]
public class DocBuilder
{
    [Key]
    public string Id { get; init; }
    
    [Required]
    public string Name { get; set; }
}