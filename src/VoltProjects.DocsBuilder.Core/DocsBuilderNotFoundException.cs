namespace VoltProjects.DocsBuilder.Core;

/// <summary>
///     <see cref="Exception"/> for when a docs builder was not found
/// </summary>
public sealed class DocsBuilderNotFoundException : Exception
{
    public DocsBuilderNotFoundException(string message)
        : base(message)
    {
    }
}