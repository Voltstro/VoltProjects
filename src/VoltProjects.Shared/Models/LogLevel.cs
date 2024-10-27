using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoltProjects.Shared.Models;

[Table("log_level")]
public class LogLevel
{
    [Key]
    public int Id { get; set; }
    
    public string Name { get; set; }
}