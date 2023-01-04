using System;

namespace VoltProjects.Server.Services.Git;

/// <summary>
///     <see cref="Exception"/> related to git tags
/// </summary>
public sealed class TagException : Exception
{
    public TagException(string message)
        : base(message)
    {
    }
}