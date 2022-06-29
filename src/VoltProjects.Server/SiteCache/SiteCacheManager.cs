using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using VoltProjects.DocsBuilder.Core;
using VoltProjects.Server.Config;
using VoltProjects.Server.Core.Git;
using VoltProjects.Server.Core.MiddlewareManagement;

namespace VoltProjects.Server.SiteCache;

/// <summary>
///     Manager for updating sites
/// </summary>
public class SiteCacheManager
{
    private readonly ILogger<SiteCacheManager> _logger;
    private readonly VoltProjectsConfig _config;
    private readonly RuntimeMiddlewareService _runtimeMiddlewareService;
    private readonly Git _git;
    private readonly DocsBuilder.Core.DocsBuilder _docsBuilder;
    private readonly List<VoltProject> _configuredProjects = new();

    public SiteCacheManager(ILogger<SiteCacheManager> logger, IOptions<VoltProjectsConfig> config, 
        RuntimeMiddlewareService runtimeMiddleware,
        Git git, 
        DocsBuilder.Core.DocsBuilder docsBuilder)
    {
        _logger = logger;
        _config = config.Value;
        _runtimeMiddlewareService = runtimeMiddleware;
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
            
            //Get our destination folder
            string destinationDir =
                Path.GetFullPath($"{AppContext.BaseDirectory}/{_config.SitesServingDir}/{project.Name}");
            
            //Clone repo
            _logger.LogInformation("Updating site cache {ProjectName}...", project.Name);
            _git.CloneRepo(project.GitUrl.ToString(), project.GitBranch, fullProjectDirectory);

            //Set to latest tag
            if (project.GitUseLatestTag)
            {
                try
                {
                    _git.SetToLatestTag(fullProjectDirectory);
                }
                catch (TagException ex)
                {
                    _logger.LogWarning("Something went wrong while setting the repo's tag! I will continue but" +
                                       "the repo will be using the latest commit on {Branch}! {Message}", project.GitBranch, ex.Message);
                }
            }
            
            //Check what commit we have built against
            string commitFile = Path.GetFullPath($"{destinationDir}/.commit");
            if (File.Exists(commitFile))
            {
                string commit = File.ReadAllText(commitFile);
                if (_git.GetRepoCommitHash(fullProjectDirectory) == commit)
                {
                    _logger.LogInformation("Commit hashes are the same, not rebuilding.");
                    Cleanup();
                    ConfigureServer();
                    continue;
                }
            }

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
            
            //TODO: We should compare, and copy/delete based of that
            //If the built site directory folder already exists, delete it so we can start from scratch
            if (Directory.Exists(destinationDir))
                Directory.Delete(destinationDir, true);

            Directory.CreateDirectory(destinationDir);
            
            //Copy built site to destination
            IOHelper.CopyDirectory(builtDocsDist, destinationDir, true);
            
            //Write commit info
            File.WriteAllText(commitFile, _git.GetRepoCommitHash(fullProjectDirectory));

            //Cleanup
            Cleanup();

            ConfigureServer();
            
            _logger.LogInformation("Project {ProjectName} has successfully been built and deployed!", project.Name);

            void Cleanup()
            {
                Directory.Delete(fullProjectDirectory, true);
            }

            void ConfigureServer()
            {
                //Setup our file server
                if (_configuredProjects.Contains(project)) 
                    return;
                
                _runtimeMiddlewareService.Configure(app =>
                {
                    app.UseFileServer(new FileServerOptions
                    {
                        FileProvider =
                            new PhysicalFileProvider(
                                Path.Combine(AppContext.BaseDirectory, $"Sites/{project.Name}")),
                        RequestPath = $"/{project.Name}",
                        RedirectToAppendTrailingSlash = true,
                        StaticFileOptions =
                        {
                            OnPrepareResponse = ctx =>
                            {
                                ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                                    "public,max-age=" + _config.HostCacheTime;
                            }
                        }
                    });
                });
                _configuredProjects.Add(project);
            }
        }
    }
}