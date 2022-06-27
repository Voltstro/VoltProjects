using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VoltProjects.DocsBuilder.Core;
using VoltProjects.Server.Config;
using VoltProjects.Server.Core.Git;

namespace VoltProjects.Server.SiteCache;

/// <summary>
///     Manager for updating sites
/// </summary>
public class SiteCacheManager
{
    private readonly ILogger<SiteCacheManager> _logger;
    private readonly VoltProjectsConfig _config;
    private readonly Git _git;
    private readonly DocsBuilder.Core.DocsBuilder _docsBuilder;

    public SiteCacheManager(ILogger<SiteCacheManager> logger, IOptions<VoltProjectsConfig> config, Git git, DocsBuilder.Core.DocsBuilder docsBuilder)
    {
        _logger = logger;
        _config = config.Value;
        _git = git;
        _docsBuilder = docsBuilder;
    }

    public void UpdateCache()
    {
        _logger.LogInformation("Updating sites cache...");

        //Make sure our path for building these sites exist
        string workPathFull = Path.GetFullPath($"{AppContext.BaseDirectory}/{_config.SitesBuildDir}");
        if (!Directory.Exists(workPathFull))
            Directory.CreateDirectory(workPathFull);
        
        _logger.LogDebug("Building sites to {BuildDir}", workPathFull);
        
        foreach (VoltProject project in _config.Projects)
        {
            //Setup our project's directory
            string fullProjectDirectory = Path.GetFullPath($"{workPathFull}/{project.Name}");
            if (!Directory.Exists(fullProjectDirectory))
                Directory.CreateDirectory(fullProjectDirectory);
            
            //Clone repo
            _logger.LogInformation("Updating site cache {ProjectName}...", project.Name);
            _git.CloneRepo(project.GitUrl.ToString(), project.GitBranch, fullProjectDirectory);
            
            //Get docs path
            string projectDocsPath = Path.GetFullPath($"{fullProjectDirectory}/{project.DocsPath}");
            if (!Directory.Exists(projectDocsPath))
            {
                _logger.LogError("Docs directory was not found in project!");
                Cleanup();
                continue;
            }
            
            //Build docs
            try
            {
                _docsBuilder.BuildDocs(fullProjectDirectory, projectDocsPath);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError("A required file was not found! {Message}", ex.Message);
                Cleanup();
                continue;
            }
            catch (JsonException ex)
            {
                _logger.LogError("An error occured while parsing JSON! {Message}", ex.Message);
                Cleanup();
                continue;
            }
            catch (DocsBuilderNotFoundException)
            {
                _logger.LogError("Docs Builder for this docs type was not found!");
                Cleanup();
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogError("An unknown error occured while building docs! {Message} {StackTrace}", ex.Message, ex.StackTrace);
                Cleanup();
                continue;
            }
            
            //Copy our site
            string builtDocsDist = Path.GetFullPath($"{projectDocsPath}/{project.DocsBuildDist}");
            if (!Directory.Exists(builtDocsDist))
            {
                _logger.LogError("Directory for built site was not found!");
                Cleanup();
                continue;
            }

            _logger.LogInformation("Copying built site to serving folder...");
            string destinationDir =
                Path.GetFullPath($"{AppContext.BaseDirectory}/{_config.SitesServingDir}/{project.Name}");
            if (Directory.Exists(destinationDir))
                Directory.Delete(destinationDir, true);

            Directory.CreateDirectory(destinationDir);
            
            IOHelper.CopyDirectory(builtDocsDist, destinationDir, true);

            //Cleanup
            Cleanup();
            
            _logger.LogInformation("Project {ProjectName} has successfully been built and deployed!", project.Name);

            void Cleanup()
            {
                Directory.Delete(fullProjectDirectory, true);
            }
        }
    }
}