using System.Linq;
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
        ProjectId = projectVersion.ProjectId;
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

        ProjectPreBuild[] preBuilds = projectVersion.PreBuilds.ToArray();
        PreBuildCommands = new ProjectPreBuildPageModel[preBuilds.Length];
        for (int i = 0; i < preBuilds.Length; i++)
        {
            PreBuildCommands[i] = new ProjectPreBuildPageModel(preBuilds[i]);
        }

        Published = projectVersion.Published;
    }
    
    public new int? Id { get; set; }
    
    public DocBuilder[] DocBuilders { get; set; }
    public Language[] Languages { get; set; }
    
    public ProjectPreBuildPageModel[] PreBuildCommands { get; set; }
    
    public bool? Success { get; set; }
}