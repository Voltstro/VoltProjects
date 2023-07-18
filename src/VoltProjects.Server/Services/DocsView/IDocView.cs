using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.FileProviders;
using VoltProjects.Server.Models;

namespace VoltProjects.Server.Services.DocsView;

internal interface IDocView
{
    public string Name { get; }
    
    public ViewResult? GetViewFromFile(ViewDataDictionary viewData, IFileProvider fileProvider, string requestPath, string sitePath, string potentialFile, string project);
}