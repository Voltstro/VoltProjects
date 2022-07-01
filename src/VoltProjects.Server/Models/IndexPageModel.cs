using System;
using VoltProjects.Server.Core.SiteCache.Config;

namespace VoltProjects.Server.Models;

public class IndexPageModel
{
    public VoltProject[] Projects { get; init; } = Array.Empty<VoltProject>();
}