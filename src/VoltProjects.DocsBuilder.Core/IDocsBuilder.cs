namespace VoltProjects.DocsBuilder.Core;

/// <summary>
///     base interface for a docs builder
/// </summary>
public interface IDocsBuilder
{
    /// <summary>
    ///     The lookup name of the builder
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Build docs at this path
    /// </summary>
    /// <param name="docsPath"></param>
    public void Build(string docsPath);
}