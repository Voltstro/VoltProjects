using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using VoltProjects.Server.Shared;

namespace VoltProjects.Server.Services.DocsServer;

public sealed class DocsServerService
{
    private readonly VoltProjectsConfig config;
    private readonly IContentTypeProvider typeProvider;
    private readonly IFileProvider fileProvider;
    
    public DocsServerService(IOptions<VoltProjectsConfig> config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        this.config = config.Value;
        fileProvider = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, this.config.SitesServingDir));
        typeProvider = new FileExtensionContentTypeProvider();
    }

    public ContentResult? TryGetProjectFile(string projectName, string? path)
    {
        VoltProject? project = config.Projects.SingleOrDefault(x =>
            x.Name.Equals(projectName, StringComparison.InvariantCultureIgnoreCase));

        if (project == null)
            return null;

        if (string.IsNullOrWhiteSpace(path))
            path = "index.html";

        string fullPath = Path.Combine(projectName, path);

        IFileInfo file = fileProvider.GetFileInfo(fullPath);
        if (!file.Exists)
            return null;
        
        string? fullFilePath = file.PhysicalPath;
        if (string.IsNullOrWhiteSpace(fullFilePath))
            return null;
        
        string? fileMimeType = null;
        if (typeProvider.TryGetContentType(fullFilePath, out string? type))
        {
            fileMimeType = type;
        }

        return new ContentResult
        {
            Content = File.ReadAllText(fullFilePath),
            ContentType = fileMimeType
        };
    }
}