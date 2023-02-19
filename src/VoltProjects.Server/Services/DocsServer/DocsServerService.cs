using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using VoltProjects.Server.Models;
using VoltProjects.Server.Services.DocsView;
using VoltProjects.Server.Shared;

namespace VoltProjects.Server.Services.DocsServer;

public sealed class DocsServerService
{
    private readonly IDbContextFactory<VoltProjectsDbContext> dbContext;
    private readonly DocsViewService docsViewService;
    private readonly IContentTypeProvider typeProvider;
    private readonly IFileProvider fileProvider;
    
    public DocsServerService(IOptions<VoltProjectsConfig> config, IDbContextFactory<VoltProjectsDbContext> dbContext, DocsViewService docsViewService)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));
        
        this.dbContext = dbContext;
        this.docsViewService = docsViewService;
        fileProvider = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, config.Value.SitesServingDir));
        typeProvider = new FileExtensionContentTypeProvider();
    }

    public IActionResult? TryGetProjectFile(HttpRequest request, ViewDataDictionary viewData, string projectName, string? version, string? path)
    {
        using VoltProjectsDbContext context = dbContext.CreateDbContext();
        Project? project = context.Projects.SingleOrDefault(x => x.Name == projectName);
        if (project == null)
            return null;

        //If we have no version, use "latest"
        if (string.IsNullOrWhiteSpace(version))
            version = "latest";

        ProjectVersion? projectVersion = context.ProjectVersions.SingleOrDefault(x => x.ProjectId == project.Id && x.VersionTag == version);
        if (projectVersion == null)
        {
            string redirectPath = $"/{Path.Combine(projectName, "latest", version, path ?? string.Empty)}/";
            return new RedirectResult(redirectPath);
        }

        //No path, or file? Assume index.html
        string? preFuckedPath = path;
        if (string.IsNullOrWhiteSpace(path))
            path = "index.html";
        else if (!Path.HasExtension(path) && path.EndsWith("/"))
            path += "index.html";
        else if (!Path.HasExtension(path) && !path.EndsWith("/"))
            path += "/index.html";

        string sitePath = Path.Combine(projectName, projectVersion.VersionTag);
        string fullPath = Path.Combine(sitePath, path);

        //Make sure path ends with a /
        if (Path.GetFileName(fullPath) == "index.html" && !request.Path.Value.EndsWith("/"))
        {
            string redirectPath = $"/{Path.Combine(projectName, version, preFuckedPath ?? string.Empty)}/";
            return new RedirectResult(redirectPath);
        }

        //View service might be able to serve this
        if (Path.GetExtension(fullPath) == ".html")
        {
            ViewResult? viewResult = docsViewService.GetViewFromDocsView(projectVersion.DocViewId, viewData, fileProvider, sitePath, path);
            if (viewResult != null)
                return viewResult;
        }

        //Try to serve the file our-selfs
        IFileInfo file = fileProvider.GetFileInfo(fullPath);
        if (!file.Exists)
            return null;
        
        string? fullFilePath = file.PhysicalPath;
        if (string.IsNullOrWhiteSpace(fullFilePath))
            return null;
        
        string? fileMimeType = null;
        if (typeProvider.TryGetContentType(fullFilePath, out string? type))
            fileMimeType = type;

        //Image type, need to serve as FileContentResult with bytes
        if (fileMimeType != null && fileMimeType.StartsWith("image/"))
            return new FileContentResult(File.ReadAllBytes(fullFilePath), fileMimeType);

        return new ContentResult
        {
            Content = File.ReadAllText(fullFilePath),
            ContentType = fileMimeType
        };
        
    }
}