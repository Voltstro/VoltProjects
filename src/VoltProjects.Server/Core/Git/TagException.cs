using System;

namespace VoltProjects.Server.Core.Git;

/// <summary>
///     Exception related to git tags
/// </summary>
public sealed class TagException : Exception
{
    public TagException(string message)
        : base(message)
    {
    }
}