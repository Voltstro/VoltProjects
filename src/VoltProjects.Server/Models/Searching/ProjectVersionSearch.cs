namespace VoltProjects.Server.Models.Searching;

public sealed class ProjectVersionSearch
{
    public ProjectVersionSearch(int id, string name)
    {
        Id = id;
        Name = name;
    }
    
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public bool Active { get; set; }
}