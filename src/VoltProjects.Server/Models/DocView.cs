using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Server.Models;

[Table("DocView")]
public class DocView
{
    [Key]
    public string Id { get; init; }
    
    [Required]
    public string Name { get; set; }
}