using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Models.View.Admin;

public class ProjectVersionPageModel : ProjectVersion
{
    public ProjectVersionPageModel()
    {
    }

    public ProjectVersionPageModel(ProjectVersion projectVersion)
    {
        Id = projectVersion.Id;
        ProjectId = projectVersion.Id;
        Project = projectVersion.Project;
        GitBranch = projectVersion.GitBranch;
        GitTag = projectVersion.GitTag;
        DocBuilderId = projectVersion.DocBuilderId;
        DocBuilder = projectVersion.DocBuilder;
        DocsPath = projectVersion.DocsPath;
        DocsBuiltPath = projectVersion.DocsBuiltPath;
        LanguageId = projectVersion.LanguageId;
        Language = projectVersion.Language;
        IsDefault = projectVersion.IsDefault;
        CreationTime = projectVersion.CreationTime;
        LastUpdateTime = projectVersion.LastUpdateTime;
    }
    
    public new int? Id { get; set; }
    
    public DocBuilder[] DocBuilders { get; set; }
    public Language[] Languages { get; set; }
    
    public bool? Success { get; set; }
}