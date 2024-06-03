using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using VoltProjects.Server.Models.Searching;

namespace VoltProjects.Server.Shared;

/// <summary>
///     <see cref="TagHelper"/> for building paginated lists
/// </summary>
[HtmlTargetElement("nav", Attributes = "page-model")]
public sealed class PageLinkTagHelper : TagHelper
{
    private IUrlHelperFactory urlHelperFactory;

    public PageLinkTagHelper(IUrlHelperFactory helperFactory)
    {
        urlHelperFactory = helperFactory;
    }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }
    public SearchPagedResult PageModel { get; set; }
    public string PageAction { get; set; }
    
    /// <summary>
    ///     CSS class for the UL element holding everything
    /// </summary>
    public string PaginationClass { get; set; } = "pagination";

    /// <summary>
    ///     CSS class for the li element holding the link
    /// </summary>
    public string PageItemClass { get; set; } = "page-item";

    /// <summary>
    ///     CSS class for the active li element
    /// </summary>
    public string PageItemActiveClass { get; set; } = "active";

    /// <summary>
    ///     CSS class for the a element link
    /// </summary>
    public string PageLinkClass { get; set; } = "page-link";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        string currentParameters = GetUrlParameters();
        
        TagBuilder result = new("ul");
        result.AddCssClass(PaginationClass);
        
        //Add previous page link
        result.InnerHtml.AppendHtml(CreatePageLink(PageModel.Page - 1, "Previous", true, currentParameters));

        //Create links for each page
        for (int i = 1; i <= PageModel.TotalPages; i++)
            result.InnerHtml.AppendHtml(CreatePageLink(i, i.ToString(), false, currentParameters));
        
        //Add next page link
        result.InnerHtml.AppendHtml(CreatePageLink(PageModel.Page + 1, "Next", true, currentParameters));
        
        output.Content.AppendHtml(result);
    }

    private TagBuilder CreatePageLink(int page, string content, bool previousOrNext, string urlParameters)
    {
        TagBuilder liTag = new("li");
        liTag.AddCssClass(PageItemClass);

        if (page == PageModel.Page && !previousOrNext)
        {
            liTag.AddCssClass(PageItemActiveClass);
            liTag.Attributes.Add("aria-current", "page");
        }
        else if (previousOrNext)
        {
            if(page < 1 || page > PageModel.TotalPages)
                liTag.AddCssClass("disabled");
        }
        
        TagBuilder tag = new("a");
        tag.AddCssClass(PageLinkClass);

        tag.Attributes["href"] = $"/search/?page={page}&{urlParameters}";
        tag.InnerHtml.AppendHtml(content);
        
        liTag.InnerHtml.AppendHtml(tag);
        return liTag;
    }

    private string GetUrlParameters()
    {
        List<string> queryParameters = ViewContext.HttpContext.Request.QueryString.Value!.TrimStart('?').Split('&').ToList();
        for (int i = 0; i < queryParameters.Count; i++)
        {
            string queryParameter = queryParameters[i];
            if (queryParameter.StartsWith("page="))
            {
                queryParameters.Remove(queryParameter);
                break;
            }
        }

        return string.Join('&', queryParameters);
    }
}