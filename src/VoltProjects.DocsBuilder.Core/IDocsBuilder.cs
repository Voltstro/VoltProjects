namespace VoltProjects.DocsBuilder.Core;

public interface IDocsBuilder
{
    public string Name { get; }

    public void Build(string docsPath);
}