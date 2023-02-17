using System;
using System.Linq;
using VoltProjects.Server.Services.DocsBuilder.VDocFx;

namespace VoltProjects.Server.Services.DocsBuilder;

/// <summary>
///     Manager for <see cref="IDocsBuilder"/>
/// </summary>
public sealed class DocsBuilderManager
{
    private readonly IDocsBuilder[] docsBuilders;
    
    /// <summary>
    ///     Creates a new <see cref="DocsBuilderManager"/> instance
    /// </summary>
    public DocsBuilderManager()
    {
        docsBuilders = new IDocsBuilder[] { new VDocFxDocxBuilder() };
    }
    
    /// <summary>
    ///     Builds docs from a path
    /// </summary>
    /// <param name="docsBuilder"></param>
    /// <param name="docsPath"></param>
    /// <exception cref="DocsBuilderNotFoundException"></exception>
    public void BuildDocs(string docsBuilder, string docsPath)
    {
        //Find docs builder
        IDocsBuilder? builder = docsBuilders.FirstOrDefault(x => x.Name.Equals(docsBuilder, StringComparison.InvariantCultureIgnoreCase));
        if (builder == null)
            throw new DocsBuilderNotFoundException($"The builder {docsBuilder} was not found!");

        //Pass it over to the docs builder
        builder.Build(docsPath);
    }
}