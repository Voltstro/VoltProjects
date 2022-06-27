using System;

namespace VoltProjects.Server.Core.Git;

public class TagException : Exception
{
    public TagException(string message)
        : base(message)
    {
    }
}