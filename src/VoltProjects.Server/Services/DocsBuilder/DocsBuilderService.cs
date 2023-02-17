using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoltProjects.Server.Models;
using VoltProjects.Server.Services.Git;
using VoltProjects.Server.Services.Robots;
using VoltProjects.Server.Shared;
using VoltProjects.Server.Shared.Helpers;

namespace VoltProjects.Server.Services.DocsBuilder;

public class DocsBuilderService
{
    private readonly ILogger<DocsBuilderService> logger;
    private readonly VoltProjectsConfig config;
    private readonly IWebHostEnvironment environment;
    private readonly IDbContextFactory<VoltProjectsDbContext> dbContext;
    private readonly SitemapService sitemapService;
    private readonly GitService git;
    private readonly DocsBuilderManager docsBuilderManager;
    
    public DocsBuilderService(ILogger<DocsBuilderService> logger,
        IOptions<VoltProjectsConfig> config, 
        IWebHostEnvironment environment,
        IDbContextFactory<VoltProjectsDbContext> dbContext,
        SitemapService sitemapService,
        GitService git)
    {
        this.logger = logger;
        this.config = config.Value;
        this.environment = environment;
        this.dbContext = dbContext;
        this.sitemapService = sitemapService;
        this.git = git;
        docsBuilderManager = new DocsBuilderManager();
    }

    public void UpdateOrBuildAllProjects(string workPath, string sitePath)
    {
        using VoltProjectsDbContext context = dbContext.CreateDbContext();
        Parallel.ForEach(context.Projects, project =>
        {
            logger.LogInformation("Building {ProjectName}...", project.Name);
            try
            {
                UpdateOrBuildProject(workPath, sitePath, project);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured while building {ProjectName}!", project.Name);
                return;
            }

            logger.LogInformation("{ProjectName} successfully built!", project.Name);
        });
    }
    
    public void UpdateOrBuildProject(string workPath, string sitePath, Project project)
    {
        using VoltProjectsDbContext context = dbContext.CreateDbContext();
        //Get or create git repo path
        string gitWorkingPath;

        //Clone/pull if git path provided is a URL
        if (project.GitIsUrl)
        {
            gitWorkingPath = Path.Combine(workPath, project.Name);
            if (!Directory.Exists(gitWorkingPath))
                Directory.CreateDirectory(gitWorkingPath);
            
            logger.LogInformation("Storing Git repo {ProjectName} at {Path}", project.Name, gitWorkingPath);
            git.PullRepoOrCloneIfDoesntExist(project.GitUrl, null, gitWorkingPath);

            ProjectVersion[] projectVersions = context.ProjectVersions.Where(x => x.ProjectId == project.Id).ToArray();
            string currentRepoTag = git.GetRepoLatestTag(gitWorkingPath);

            foreach (ProjectVersion projectVersion in projectVersions)
            {
                if(projectVersion.VersionTag == "latest")
                    git.SetToLatestTag(gitWorkingPath);
                else
                    git.SetRepoToTag(gitWorkingPath, projectVersion.VersionTag);

                BuildProjectDocs(project, projectVersion.DocsPath, projectVersion.DocBuilderId, projectVersion.DocsBuiltPath, gitWorkingPath, projectVersion.VersionTag, sitePath);
            }
        
            git.SetRepoToTag(gitWorkingPath, currentRepoTag);
        }
        else
        {
            gitWorkingPath = project.GitUrl;
            if (!Directory.Exists(gitWorkingPath))
                throw new DirectoryNotFoundException($"The directory {gitWorkingPath} was not found!");
            
            logger.LogInformation("Using git repo {ProjectName} at {Path}", project.Name, gitWorkingPath);

            ProjectVersion? version = context.ProjectVersions.SingleOrDefault(x => x.ProjectId == project.Id && x.VersionTag == "latest");
            if (version == null)
                throw new Exception(
                    "When using local git repos, a project version with a version tag 'latest' must exist!");

            BuildProjectDocs(project, version.DocsPath, version.DocBuilderId, version.DocsBuiltPath, gitWorkingPath,
                version.VersionTag, sitePath);
        }
    }

    private void BuildProjectDocs(Project project, string docPath, string docBuilder, string docBuiltPath, string gitWorkingPath, string versionTag, string sitePath)
    {
        //Get docs path
        string docsPath = Path.Combine(gitWorkingPath, docPath);
        if (!Directory.Exists(docsPath))
            throw new DirectoryNotFoundException($"The directory {docsPath} was not found!");

        //Build docs with docs builder
        docsBuilderManager.BuildDocs(docBuilder, docsPath);

        //Copy site to our site serving folder
        string docsBuiltSitePath = Path.Combine(docsPath, docBuiltPath);
        if (!Directory.Exists(docsBuiltSitePath))
            throw new DirectoryNotFoundException($"The directory {docsBuiltSitePath} was not found!");

        string docsServingSitePath = Path.Combine(sitePath, project.Name, versionTag);
        if (Directory.Exists(docsServingSitePath))
            Directory.Delete(docsServingSitePath, true);
        
        Directory.CreateDirectory(docsServingSitePath);
        
        IOHelper.CopyDirectory(docsBuiltSitePath, docsServingSitePath, true);
    }
}