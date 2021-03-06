using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using VoltProjects.DocsBuilder.Core;
using VoltProjects.Server.Core.Collections;
using VoltProjects.Server.Core.Git;
using VoltProjects.Server.Core.Helper;
using VoltProjects.Server.Core.MiddlewareManagement;
using VoltProjects.Server.Core.SiteCache.Config;

namespace VoltProjects.Server.Core.SiteCache;

/// <summary>
///     Manager for updating sites
/// </summary>
public sealed class SiteCacheManager
{
    private readonly ILogger<SiteCacheManager> _logger;
    private readonly VoltProjectsConfig _config;
    private readonly IWebHostEnvironment _environment;
    private readonly RuntimeMiddlewareService _runtimeMiddlewareService;
    private readonly Git.Git _git;
    private readonly DocsBuilderManager _docsBuilderManager;

    public IReadOnlyList<VoltProject> ConfiguredProjects = null!;

    public SiteCacheManager(ILogger<SiteCacheManager> logger, IOptions<VoltProjectsConfig> config, 
        IWebHostEnvironment environment,
        RuntimeMiddlewareService runtimeMiddleware,
        Git.Git git, 
        DocsBuilderManager docsBuilderManager)
    {
        _logger = logger;
        _config = config.Value;
        _environment = environment;
        _runtimeMiddlewareService = runtimeMiddleware;
        _git = git;
        _docsBuilderManager = docsBuilderManager;
    }

    /// <summary>
    ///     Updates the sites be pulling down the latest updates of them from their repos, then building them
    /// </summary>
    public void UpdateCache()
    {
        _logger.LogInformation("Updating sites cache...");

        //Make sure our path for building these sites exist
        string workPathFull = Path.GetFullPath($"{AppContext.BaseDirectory}/{_config.SitesBuildDir}");
        if (!Directory.Exists(workPathFull))
            Directory.CreateDirectory(workPathFull);
        
        _logger.LogDebug("Building sites to {BuildDir}", workPathFull);
        ListBuilder<VoltProject> configuredProjectsBuilder = new();

        Parallel.ForEach(_config.Projects, project =>
        {
            //Setup our project's directory
            string fullProjectDirectory = Path.GetFullPath($"{workPathFull}/{project.Name}");
            if (!Directory.Exists(fullProjectDirectory))
                Directory.CreateDirectory(fullProjectDirectory);
            
            //Get our destination folder
            string destinationDir =
                Path.GetFullPath($"{AppContext.BaseDirectory}/{_config.SitesServingDir}/{project.Name}");
            
            //Clone or pull repo
            _logger.LogInformation("Updating site cache {ProjectName}...", project.Name);

            try
            {
                _git.PullRepoOrCloneIfDoesntExist(project.GitUrl.ToString(), project.GitBranch, fullProjectDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong while trying to pull/clone the git repo!");
                Cleanup();
                return;
            }

            //Set to latest tag
            if (project.GitUseLatestTag)
            {
                try
                {
                    _git.SetToLatestTag(fullProjectDirectory);
                }
                catch (TagException ex)
                {
                    _logger.LogWarning(ex, "Something went wrong while setting the repo's tag! I will continue but" +
                                       "the repo will be using the latest commit on {Branch}!", project.GitBranch);
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
                    ConfigureProject();
                    return;
                }
            }

            //Get docs path
            string projectDocsPath = Path.GetFullPath($"{fullProjectDirectory}/{project.DocsPath}");
            if (!Directory.Exists(projectDocsPath))
            {
                _logger.LogError("Docs directory was not found in project!");
                Cleanup();
                return;
            }
            
            //Build docs
            try
            {
                _docsBuilderManager.BuildDocs(fullProjectDirectory, projectDocsPath);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "A required file was not found!");
                Cleanup();
                return;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "An error occured while parsing JSON!");
                Cleanup();
                return;
            }
            catch (DocsBuilderNotFoundException)
            {
                _logger.LogError("Docs Builder for this docs type was not found!");
                Cleanup();
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unknown error occured while building the docs!");
                Cleanup();
                return;
            }
            
            //Copy our site
            string builtDocsDist = Path.GetFullPath($"{projectDocsPath}/{project.DocsBuildDist}");
            if (!Directory.Exists(builtDocsDist))
            {
                _logger.LogError("Directory for built site was not found!");
                Cleanup();
                return;
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
            
            //Icon
            if (project.IconPath != null)
            {
                string iconPath = Path.GetFullPath($"{fullProjectDirectory}/{project.IconPath}");
                if (!File.Exists(iconPath))
                {
                    _logger.LogWarning("Project specifies an icon, but the icon was not found!");
                }
                else
                {
                    string dest = Path.GetFullPath($"{destinationDir}/projecticon{Path.GetExtension(iconPath)}");
                    File.Copy(iconPath, dest);
                }
            }
            
            ConfigureProject();
            
            _logger.LogInformation("Project {ProjectName} has successfully been built and deployed!", project.Name);

            void Cleanup()
            {
                Directory.Delete(fullProjectDirectory, true);
            }

            void ConfigureProject()
            {
                //For setting up our file server
                if (configuredProjectsBuilder.Contains(project)) 
                    return;
                
                configuredProjectsBuilder.Add(project);
            }
        });
        
        configuredProjectsBuilder.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
        ConfiguredProjects = configuredProjectsBuilder.AsList();
    }

    public void ConfigureFileServers()
    {
        _runtimeMiddlewareService.Configure(app =>
        {
            foreach (VoltProject project in ConfiguredProjects)
            {
                app.UseFileServer(new FileServerOptions
                {
                    FileProvider =
                        new PhysicalFileProvider(
                            Path.Combine(AppContext.BaseDirectory, $"{_config.SitesServingDir}/{project.Name}")),
                    RequestPath = $"/{project.Name}",
                    RedirectToAppendTrailingSlash = true,
                    StaticFileOptions =
                    {
#if !DEBUG
                        OnPrepareResponse = ctx =>
                        {
                            if(_environment.IsDevelopment())
                                return;
                            
                            ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                                "public,max-age=" + _config.SitesServingCacheHeaderTimeSeconds;
                        }
#endif
                    }
                });
                _logger.LogDebug("Configured file server for {ProjectName}", project.Name);
            }
        });
    }
}