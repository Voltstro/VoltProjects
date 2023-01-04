using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoltProjects.DocsBuilder.Core;
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
    private readonly SitemapService sitemapService;
    private readonly GitService git;
    private readonly DocsBuilderManager docsBuilderManager;
    
    public DocsBuilderService(ILogger<DocsBuilderService> logger,
        IOptions<VoltProjectsConfig> config, 
        IWebHostEnvironment environment,
        SitemapService sitemapService,
        GitService git)
    {
        this.logger = logger;
        this.config = config.Value;
        this.environment = environment;
        this.sitemapService = sitemapService;
        this.git = git;
        docsBuilderManager = new DocsBuilderManager(DependencyContext.Default);
    }

    public void UpdateOrBuildAllProjects(string workPath, string sitePath)
    {
        Parallel.ForEach(config.Projects, project =>
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
    
    public void UpdateOrBuildProject(string workPath, string sitePath, VoltProject project)
    {
        //Get or create git repo path
        string gitRepoPath = project.GitPath;

        //Clone/pull if git path provided is a URL
        if (project.GitIsUrl)
        {
            Path.Combine(workPath, project.Name);
            if (!Directory.Exists(gitRepoPath))
                Directory.CreateDirectory(gitRepoPath);
            
            logger.LogInformation("Storing Git repo {ProjectName} at {Path}", project.Name, gitRepoPath);
            git.PullRepoOrCloneIfDoesntExist(project.GitPath, project.GitBranch, gitRepoPath);
        }
        else
        {
            if (!Directory.Exists(gitRepoPath))
                throw new DirectoryNotFoundException($"The directory {gitRepoPath} was not found!");
            
            logger.LogInformation("Using git repo {ProjectName} at {Path}", project.Name, gitRepoPath);
        }

        //Get docs path
        string docsPath = Path.Combine(gitRepoPath, project.DocsPath);
        if (!Directory.Exists(docsPath))
            throw new DirectoryNotFoundException($"The directory {docsPath} was not found!");
            
        //Build docs with docs builder
        docsBuilderManager.BuildDocs(project.DocsBuilder, docsPath);
        
        //Copy site to our site serving folder
        string docsBuiltSitePath = Path.Combine(gitRepoPath, project.DocsOutputPath);
        if (!Directory.Exists(docsBuiltSitePath))
            throw new DirectoryNotFoundException($"The directory {docsBuiltSitePath} was not found!");

        string docsServingSitePath = Path.Combine(sitePath, project.Name);
        if (!Directory.Exists(docsServingSitePath))
            Directory.CreateDirectory(docsServingSitePath);
        
        IOHelper.CopyDirectory(docsBuiltSitePath, docsServingSitePath, true);
    }
}