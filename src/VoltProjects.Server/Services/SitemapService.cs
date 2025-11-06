using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using VoltProjects.Server.Shared;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Services;

/// <summary>
///     Backing service for sitemaps
/// </summary>
public sealed class SitemapService
{
    private static readonly XNamespace SitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private static readonly string[] BaseSitePaths = ["/", "/about/"];
    
    private readonly VoltProjectDbContext dbContext;
    private readonly IMemoryCache memoryCache;
    private readonly VoltProjectsConfig config;

    private readonly Uri siteBaseUrl;
    
    public SitemapService(VoltProjectDbContext dbContext, IMemoryCache memoryCache, IOptions<VoltProjectsConfig> config)
    {
        this.dbContext = dbContext;
        this.memoryCache = memoryCache;
        this.config = config.Value;

        siteBaseUrl = new Uri(this.config.SiteUrl);
    }

    /// <summary>
    ///     Gets the site's sitemap
    /// </summary>
    public async Task<string> GetSiteSitemap()
    {
       const string cacheKey = "SiteSitemap";
       string? sitemapDocument = await memoryCache.GetOrCreateAsync<string>(cacheKey, async entry =>
       {
           XElement baseSitemapRoot = new(SitemapNamespace + "urlset");

           //Add VoltProject's pages
           foreach (string baseSitePath in BaseSitePaths)
           {
               baseSitemapRoot.Add(CreatePageLoc(baseSitePath));
           }

           entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(31);
           
           XDocument document = new(baseSitemapRoot);
           return XDocumentToString(document);
       });

       return sitemapDocument!;
    }
    
    /// <summary>
    ///     Gets the site's index sitemap
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetSiteIndexSitemap()
    {
        const string cacheKey = "SiteIndexSitemap";
        string? indexSitemapDocument = await memoryCache.GetOrCreateAsync<string>(cacheKey, async entry =>
        {
            //Create root element, and add main site's sitemap
            XElement indexSitemapRoot = new(SitemapNamespace + "sitemapindex");
            indexSitemapRoot.Add(CreatePageLoc("sitemap.xml", null, "sitemap"));
            
            //Get all projects and add to index sitemap
            await foreach (ProjectVersion projectVersion in GetProjectVersionsQuery(dbContext))
            {
                string fullPath = Path.Combine(projectVersion.Project.Name, projectVersion.VersionTag, "sitemap.xml");
                indexSitemapRoot.Add(CreatePageLoc(fullPath, null, "sitemap"));
            }
            
            entry.AbsoluteExpirationRelativeToNow = config.SitemapCacheExpiration;
            
            XDocument document = new(indexSitemapRoot);
            return XDocumentToString(document);
        });

        return indexSitemapDocument!;
    }

    /// <summary>
    ///     Gets a project's site map
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    public async Task<string?> GetProjectSitemap(string projectName, string version)
    {
        string cacheKey = $"ProjectSitemap-{projectName}-{version}";
        string? sitemapDocument = await memoryCache.GetOrCreateAsync<string?>(cacheKey, async entry =>
        {
            ProjectVersion? projectVersion = await GetProjectVersionQuery(dbContext, projectName, version);

            if (projectVersion == null)
                return null;

            XElement urlSet = new(SitemapNamespace + "urlset");

            await foreach (ProjectPage projectPage in GetProjectPagesQuery(dbContext, projectVersion.Id))
            {
                string path = projectPage.Path == "." ? string.Empty : projectPage.Path;
                string fullPath = Path.Combine(projectVersion.Project.Name, projectVersion.VersionTag, path);
                if (path == string.Empty)
                    fullPath += "/";
                
                urlSet.Add(CreatePageLoc(fullPath, projectPage.PublishedDate));
            }

            entry.AbsoluteExpirationRelativeToNow = config.SitemapCacheExpiration;

            //Create document and use utf-8 encoding
            XDocument document = new(urlSet);
            return XDocumentToString(document);
        });

        return sitemapDocument;
    }
    
    private XElement CreatePageLoc(string pagePath, DateTime? lastModTime = null, string element = "url")
    {
        Uri fullUrl =  new(siteBaseUrl, pagePath);
        string loc = fullUrl.ToString();
        
        XElement urlElement = new(SitemapNamespace + element);
        urlElement.Add(new XElement(SitemapNamespace + "loc", loc));
        
        if(lastModTime.HasValue)
            urlElement.Add(new XElement(SitemapNamespace + "lastmod", lastModTime.Value.ToString("yyyy-MM-dd")));
        
        return urlElement;
    }

    private static string XDocumentToString(XDocument document)
    {
        document.Declaration = new XDeclaration("1.0", "utf-8", null);
        StringWriter stringWriter = new Utf8StringWriter();
        document.Save(stringWriter, SaveOptions.DisableFormatting);
        return stringWriter.ToString();
    }
    
    private class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

    //Get project versions query
    private static readonly Func<VoltProjectDbContext, IAsyncEnumerable<ProjectVersion>> GetProjectVersionsQuery =
        EF.CompileAsyncQuery(
            (VoltProjectDbContext dbContext) =>
                dbContext.ProjectVersions
                    .AsNoTracking()
                    .Include(x => x.Project)
                    .Where(x => 
                        x.Published &&
                        x.SeIndexable));
    
    //Get project version query
    private static readonly Func<VoltProjectDbContext, string, string, Task<ProjectVersion?>> GetProjectVersionQuery =
        EF.CompileAsyncQuery(
            (VoltProjectDbContext dbContext, string projectName, string versionTag) =>
                dbContext.ProjectVersions
                    .AsNoTracking()
                    .Include(x => x.Project)
                    .FirstOrDefault(x =>
                        x.VersionTag == versionTag && 
                        x.Project.Name == projectName && 
                        x.Published && 
                        x.SeIndexable &&
                        x.Project.Published));

    private static readonly Func<VoltProjectDbContext, int, IAsyncEnumerable<ProjectPage>> GetProjectPagesQuery =
        EF.CompileAsyncQuery(
            (VoltProjectDbContext dbContext, int projectVersionId) => 
                dbContext.ProjectPages
                    .AsNoTracking()
                    .Where(x => x.Published && x.ProjectVersionId == projectVersionId));
}