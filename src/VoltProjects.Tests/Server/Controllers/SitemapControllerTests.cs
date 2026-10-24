using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using VoltProjects.Server.Controllers;
using VoltProjects.Server.Services;
using VoltProjects.Server.Shared;
using VoltProjects.Shared.Models;
using VoltProjects.Tests.Core;
using VoltProjects.Tests.Utils;

namespace VoltProjects.Tests.Server.Controllers;

/// <summary>
///     Tests for <see cref="SitemapController"/>
/// </summary>
public class SitemapControllerTests : DbContextTest
{
    private VoltProjectsConfig config;
    private SitemapController sitemapController;
    
    public override void Init()
    {
        base.Init();
        
        config = ConfigUtils.CreateServerConfig().Value;
        SitemapService sitemapService = ServiceUtils.CreateSitemapService(DbContext);
        sitemapController = new SitemapController(sitemapService);
    }

    public override void Cleanup()
    {
        sitemapController.Dispose();
        
        base.Cleanup();
    }

    /// <summary>
    ///     Test index sitemap, which has links to the other sitemaps
    /// </summary>
    [Test]
    public async Task SitemapGetIndexSitemapTest()
    {
        //Setup setup data
        ProjectVersion[] projects =
        [
            DbContext.CreateProject("Test1", "Test1 Project", "latest"),
            DbContext.CreateProject("Test2", "Test2 Project", "latest")
        ];
        
        //Do action and assert results
        IActionResult result = await sitemapController.GetIndexSitemap();
        Assert.That(result, Is.TypeOf<ContentResult>());
        ContentResult? contentResult = result as ContentResult;
        
        Assert.That(contentResult, Is.Not.Null);
        Assert.That(contentResult.ContentType, Is.EqualTo("application/xml"));
        Assert.That(contentResult.Content, Is.Not.Null);
        
        XDocument document = XDocument.Parse(contentResult.Content);
        Assert.That(document.Root, Is.Not.Null);
        Assert.That(document.Root.Name, Is.EqualTo(XName.Get("sitemapindex", SitemapService.SitemapNamespace.NamespaceName)));
        
        XElement[] sitemaps = document.Root.Elements().ToArray();
        Assert.That(sitemaps, Has.Length.EqualTo(projects.Length + 1));
        for (int i = 0; i < sitemaps.Length; i++)
        {
            XElement sitemap = sitemaps[i];
            Assert.That(sitemap.Name, Is.EqualTo(XName.Get("sitemap", SitemapService.SitemapNamespace.NamespaceName)));
            XElement? locElement = sitemap.Element(XName.Get("loc", SitemapService.SitemapNamespace.NamespaceName));
            Assert.That(locElement, Is.Not.Null);

            //First element is the main base sitemap
            string sitemapPath;
            if (i == 0)
            {
                sitemapPath = "sitemap.xml";
            }
            else
            {
                ProjectVersion projectVersion = projects[i - 1];
                sitemapPath = Path.Combine(projectVersion.Project.Name, projectVersion.VersionTag, "sitemap.xml");
            }
            
            Assert.That(locElement.Value, Is.EqualTo(new Uri(new Uri(config.SiteUrl), sitemapPath).ToString()));
        }
    }
    
    /// <summary>
    ///     Test base sitemap, which should just have the main pages
    /// </summary>
    [Test]
    public async Task SitemapBaseSitemapTest()
    {
        //Do action and assert results
        IActionResult result = await sitemapController.GetBaseSitemap();
        Assert.That(result, Is.TypeOf<ContentResult>());
        ContentResult? contentResult = result as ContentResult;
        
        Assert.That(contentResult, Is.Not.Null);
        Assert.That(contentResult.ContentType, Is.EqualTo("application/xml"));
        Assert.That(contentResult.Content, Is.Not.Null);
        
        XDocument document = XDocument.Parse(contentResult.Content);
        Assert.That(document.Root, Is.Not.Null);
        Assert.That(document.Root.Name, Is.EqualTo(XName.Get("urlset", SitemapService.SitemapNamespace.NamespaceName)));
        
        XElement[] urls = document.Root.Elements().ToArray();
        Assert.That(urls, Has.Length.EqualTo(SitemapService.BaseSitePaths.Length));
        for (int i = 0; i < urls.Length; i++)
        {
            XElement url = urls[i];
            Assert.That(url.Name, Is.EqualTo(XName.Get("url", SitemapService.SitemapNamespace.NamespaceName)));
            XElement? locElement = url.Element(XName.Get("loc", SitemapService.SitemapNamespace.NamespaceName));
            Assert.That(locElement, Is.Not.Null);
            Assert.That(locElement.Value, Is.EqualTo(new Uri(new Uri(config.SiteUrl), SitemapService.BaseSitePaths[i]).ToString()));
        }
    }

    /// <summary>
    ///     Test project sitemap
    /// </summary>
    [Test]
    public async Task SitemapProjectSitemapTest()
    {
        //Setup setup data
        ProjectVersion project = DbContext.CreateProject("Test", "Test Project", "latest");
        ProjectPage[] pages =
        [
            DbContext.CreateProjectPage(project, ".", "Test Page", "Test Page Content"),
            DbContext.CreateProjectPage(project, "test/", "Test Page2", "Test Page2 Content")
        ];

        //Do action and assert results
        IActionResult result = await sitemapController.GetProjectSitemap("Test", "latest");
        Assert.That(result, Is.TypeOf<ContentResult>());
        ContentResult? contentResult = result as ContentResult;
        
        Assert.That(contentResult, Is.Not.Null);
        Assert.That(contentResult.ContentType, Is.EqualTo("application/xml"));
        Assert.That(contentResult.Content, Is.Not.Null);
        
        XDocument document = XDocument.Parse(contentResult.Content);
        Assert.That(document.Root, Is.Not.Null);
        Assert.That(document.Root.Name, Is.EqualTo(XName.Get("urlset", SitemapService.SitemapNamespace.NamespaceName)));
        
        XElement[] urls = document.Root.Elements().ToArray();
        Assert.That(urls, Has.Length.EqualTo(2));
        for (int i = 0; i < urls.Length; i++)
        {
            XElement url = urls[i];
            Assert.That(url.Name, Is.EqualTo(XName.Get("url", SitemapService.SitemapNamespace.NamespaceName)));
            XElement? locElement = url.Element(XName.Get("loc", SitemapService.SitemapNamespace.NamespaceName));
            Assert.That(locElement, Is.Not.Null);
            ProjectPage projectPage = pages[i];
            string path = projectPage.Path == "." ? string.Empty : projectPage.Path;
            string fullPath = Path.Combine(project.Project.Name, project.VersionTag, path);
            if (path == string.Empty)
                fullPath += "/";
            
            Assert.That(locElement.Value, Is.EqualTo(new Uri(new Uri(config.SiteUrl), fullPath).ToString()));
        }
    }

    /// <summary>
    ///     Test not found project sitemap
    /// </summary>
    [Test]
    public async Task SitemapProjectSitemapNotFoundTest()
    {
        //Do action and assert results
        IActionResult result = await sitemapController.GetProjectSitemap("Test", "latest");
        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }
}