using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using VoltProjects.Server.Shared;

namespace VoltProjects.Server.Services.Robots;

public sealed class SitemapService
{
    //TODO: We should make this externally configurable
    private const string BaseUrl = "https://projects.voltstro.dev";
    
    private readonly string[] baseSitePaths = { "/", "/about" };

    public readonly ReadOnlyMemory<byte> CompressedBaseSitemap;

    private ReadOnlyMemory<byte>? compressedIndexSitemap;
    public ReadOnlyMemory<byte> CompressedIndexSitemap
    {
        get
        {
            if (!compressedIndexSitemap.HasValue)
            {
                XDocument indexSitemap = GenerateIndexSitemap();
                compressedIndexSitemap = CompressDocument(indexSitemap);
            }
            
            return compressedIndexSitemap.Value;
        }
    }

    private readonly List<VoltProject> projects = new();

    public SitemapService()
    {
        XDocument baseSitemap = GenerateBaseSitemap();
        CompressedBaseSitemap = CompressDocument(baseSitemap);
    }

    public void AddProjectSitemap(VoltProject project)
    {
        projects.Add(project);
    }

    private XDocument GenerateIndexSitemap()
    {
        XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        XElement root = new(xmlns + "sitemapindex");
        
        //VoltProject's sitemap
        XElement baseSitemap = GenerateIndexSitemapElement(xmlns, $"{BaseUrl}/sitemap.xml.gz");
        root.Add(baseSitemap);

        foreach (VoltProject project in projects)
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

        foreach (string sitePath in baseSitePaths)
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

        byte[] buffer = new byte[documentStream.Position];
        documentStream.Position = 0;
        int _ = documentStream.Read(buffer, 0, buffer.Length);
        
        gzStream.Write(buffer);
        gzStream.Close();

        byte[] compressedData = documentCompressedStream.ToArray();
        
        gzStream.Dispose();
        documentCompressedStream.Dispose();
        documentStream.Dispose();
        
        return compressedData;
    }
}