using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VoltProjects.Server.Models;
using VoltProjects.Server.Models.View;
using VoltProjects.Server.Services;
using VoltProjects.Server.Shared;

namespace VoltProjects.Server.Components;

[ViewComponent(Name = "MetaTags")]
public class MetaTagsViewComponent : ViewComponent
{
    private readonly VoltProjectsConfig vpConfig;
    private readonly StructuredDataService structuredDataService;
    
    public MetaTagsViewComponent(IOptions<VoltProjectsConfig> config, StructuredDataService structuredDataService)
    {
        vpConfig = config.Value;
        this.structuredDataService = structuredDataService;
    }
    
    public Task<IViewComponentResult> InvokeAsync()
    {
        Uri baseUri = new(vpConfig.SiteUrl);
        
        bool? isError = ViewBag.IsError;
        Uri? fullUrl = null;
        if(!isError.HasValue)
        {
            string requestPath = Request.Path;
            fullUrl = new Uri(baseUri, requestPath);
        }
        
        return Task.FromResult<IViewComponentResult>(View("MetaTags", new MetaTagsViewModel
        {
            StructuredDataJson = structuredDataService.StructuredDataJson,
            RequestPath = fullUrl
        }));
    }
}