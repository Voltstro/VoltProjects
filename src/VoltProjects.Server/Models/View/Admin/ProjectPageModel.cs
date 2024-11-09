using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Models.View.Admin;

/// <summary>
///     View model for the /project/edit|new/ page.
/// </summary>
public class ProjectPageModel : Project
{
    public ProjectPageModel()
    {
    }
    
    public ProjectPageModel(Project project)
    {
        Id = project.Id;
        Name = project.Name;
        DisplayName = project.DisplayName;
        ShortName = project.ShortName;
        Description = project.Description;
        GitUrl = project.GitUrl;
        IconPath = project.IconPath;
        LastUpdateTime = project.LastUpdateTime;
        CreationTime = project.CreationTime;
    }
    
    public new int? Id { get; set; }

    public IFormFile? UploadFile { get; set; }
    
    public bool? Success { get; set; }
}