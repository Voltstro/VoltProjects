using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VoltProjects.Server.Services;

/// <summary>
///     Service for storing global sitemap
/// </summary>
public sealed class SitemapService
{
    private readonly object sitemapLock;

    private byte[] compressedIndexSitemap;
    private byte[] compressedBaseSitemap;
    private readonly Dictionary<string, byte[]> compressedProjectSitemaps;

    private bool hasGenerated;

    public SitemapService()
    {
        compressedProjectSitemaps = new Dictionary<string, byte[]>();
        sitemapLock = new object();
    }

    public async Task<byte[]> GetIndexSitemap(CancellationToken cancellationToken)
    {
        //TODO: Something better then doing this?
        while (!hasGenerated)
        {
            await Task.Delay(25, cancellationToken);
        }
        
        lock (sitemapLock)
        {
            return compressedIndexSitemap;
        }
    }

    public async Task<byte[]> GetBaseSitemap(CancellationToken cancellationToken)
    {
        //TODO: Something better then doing this?
        while (!hasGenerated)
        {
            await Task.Delay(25, cancellationToken);
        }
        
        lock (sitemapLock)
        {
            return compressedBaseSitemap;
        }
    }

    public async Task<byte[]?> GetProjectSitemap(string name, string version, CancellationToken cancellationToken)
    {
        //TODO: Something better then doing this?
        while (!hasGenerated)
        {
            await Task.Delay(25, cancellationToken);
        }

        lock (sitemapLock)
        {
            string key = $"{name}/{version}";
            KeyValuePair<string, byte[]>? sitemap = compressedProjectSitemaps.FirstOrDefault(x => x.Key == key);
            return sitemap?.Value;
        }
    }

    public void SetSitemaps(XDocument indexSitemapDocument, XDocument baseSitemapDocument, Dictionary<string, XDocument> projectSitemaps)
    {
        lock (sitemapLock)
        {
            compressedIndexSitemap = CompressDocument(indexSitemapDocument);
            compressedBaseSitemap = CompressDocument(baseSitemapDocument);

            foreach (KeyValuePair<string,XDocument> projectSitemap in projectSitemaps)
            {
                KeyValuePair<string, byte[]>? foundResult = compressedProjectSitemaps.FirstOrDefault(x => x.Key == projectSitemap.Key);
                if (!foundResult.HasValue)
                    compressedProjectSitemaps.Remove(foundResult.Value.Key);
                
                compressedProjectSitemaps.Add(projectSitemap.Key, CompressDocument(projectSitemap.Value));
            }
        }

        hasGenerated = true;
    }

    private static byte[] CompressDocument(XDocument document)
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