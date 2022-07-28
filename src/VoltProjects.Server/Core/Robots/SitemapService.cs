using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using VoltProjects.Server.Core.SiteCache.Config;

namespace VoltProjects.Server.Core.Robots;

public class SitemapService
{
    //TODO: We should make this externally configurable
    private const string BaseUrl = "https://projects.voltstro.dev";
    
    private readonly string[] _baseSitePaths = { "/", "/about" };

    public readonly ReadOnlyMemory<byte> CompressedBaseSitemap;

    private ReadOnlyMemory<byte>? _compressedIndexSitemap;
    public ReadOnlyMemory<byte> CompressedIndexSitemap
    {
        get
        {
            if (!_compressedIndexSitemap.HasValue)
            {
                XDocument indexSitemap = GenerateIndexSitemap();
                _compressedIndexSitemap = CompressDocument(indexSitemap);
            }
            
            return _compressedIndexSitemap.Value;
        }
    }

    private readonly List<VoltProject> _projects = new();

    public SitemapService()
    {
        XDocument baseSitemap = GenerateBaseSitemap();
        CompressedBaseSitemap = CompressDocument(baseSitemap);
    }

    public void AddProjectSitemap(VoltProject project)
    {
        if(!project.HasSitemap)
            return;
        
        _projects.Add(project);
    }

    private XDocument GenerateIndexSitemap()
    {
        XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        XElement root = new(xmlns + "sitemapindex");
        
        //VoltProject's sitemap
        XElement baseSitemap = GenerateIndexSitemapElement(xmlns, $"{BaseUrl}/sitemap.xml.gz");
        root.Add(baseSitemap);

        foreach (VoltProject project in _projects)
        {
            string fullUrl = $"{BaseUrl}/{project.Name}/sitemap.xml.gz";
            XElement sitemapElement = GenerateIndexSitemapElement(xmlns, fullUrl);
            root.Add(sitemapElement);
        }

        return new XDocument(root);
    }

    private static XElement GenerateIndexSitemapElement(XNamespace xmlns, string fullUrl)
    {
        XElement sitemapElement = new(
            xmlns + "sitemap",
            new XElement(xmlns + "loc", fullUrl));
        
        return sitemapElement;
    }

    private XDocument GenerateBaseSitemap()
    {
        XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        XElement root = new(xmlns + "urlset");

        foreach (string sitePath in _baseSitePaths)
        {
            string fullUrl = $"{BaseUrl}{sitePath}";
            XElement urlElement = new(
                xmlns + "url",
                new XElement(xmlns + "loc", fullUrl));
            
            root.Add(urlElement);
        }

        return new XDocument(root);
    }

    private static ReadOnlyMemory<byte> CompressDocument(XDocument document)
    {
        MemoryStream documentStream = new();
        document.Save(documentStream);

        MemoryStream documentCompressedStream = new();
        GZipStream gzStream = new(documentCompressedStream, CompressionLevel.SmallestSize);

        gzStream.Write(documentStream.GetBuffer());
        gzStream.Close();

        byte[] compressedData = documentCompressedStream.ToArray();
        
        gzStream.Dispose();
        documentCompressedStream.Dispose();
        documentStream.Dispose();
        
        return compressedData;
    }
}