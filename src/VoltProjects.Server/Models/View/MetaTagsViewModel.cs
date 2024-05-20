using System;

namespace VoltProjects.Server.Models.View;

public struct MetaTagsViewModel
{
    public string StructuredDataJson { get; init; }
    public Uri? RequestPath { get; init; }
}