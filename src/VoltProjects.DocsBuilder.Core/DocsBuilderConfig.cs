namespace VoltProjects.DocsBuilder.Core;

public class DocsBuilderConfig
{
    public string DocsType { get; set; } = string.Empty;

    public DocsBuilderAction[] PreActions { get; set; } = Array.Empty<DocsBuilderAction>();
}