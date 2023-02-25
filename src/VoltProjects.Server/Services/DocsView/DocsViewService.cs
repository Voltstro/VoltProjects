using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.FileProviders;
using VoltProjects.Server.Models;
using VoltProjects.Server.Services.DocsView.VDocFx;

namespace VoltProjects.Server.Services.DocsView;

public class DocsViewService
{
    private readonly List<IDocView> docViews;

    public DocsViewService()
    {
        docViews = new List<IDocView> { new VDocFxView() };
    }

    public ViewResult? GetViewFromDocsView(string docsView, ViewDataDictionary viewData, IFileProvider fileProvider, string site, string potentialFile, string project)
    {
        IDocView? docView = docViews.Find((x) => x.Name == docsView);
        return docView?.GetViewFromFile(viewData, fileProvider, site, potentialFile, project);
    }
}