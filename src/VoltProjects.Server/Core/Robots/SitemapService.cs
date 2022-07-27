using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace VoltProjects.Server.Core.Robots;

public class SitemapService
{
    private readonly string[] _baseSitePaths = { "/", "/about" };

    public readonly ReadOnlyMemory<byte> CompressedBaseSitemap;

    public SitemapService()
    {
        XDocument baseSitemap = GenerateBaseSitemap();
        CompressedBaseSitemap = CompressDocument(baseSitemap);
    }

    private XDocument GenerateBaseSitemap()
    {
        XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        XElement root = new(xmlns + "urlset");

        foreach (string sitePath in _baseSitePaths)
        {
            string fullUrl = $"https://projects.voltstro.dev{sitePath}";
            XElement urlElement = new XElement(
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
        
        return documentCompressedStream.ToArray();
    }
}