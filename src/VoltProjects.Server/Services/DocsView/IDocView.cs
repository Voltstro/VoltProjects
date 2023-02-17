using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.FileProviders;

namespace VoltProjects.Server.Services.DocsView;

internal interface IDocView
{
    public string Name { get; }
    
    public ViewResult? GetViewFromFile(ViewDataDictionary viewData, IFileProvider fileProvider, string sitePath, string potentialFile);
}