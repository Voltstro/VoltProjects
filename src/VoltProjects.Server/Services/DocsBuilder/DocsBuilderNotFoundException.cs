using System;

namespace VoltProjects.Server.Services.DocsBuilder;

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