using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Primitives;

namespace VoltProjects.Server.Shared.Paging;

[HtmlTargetElement("div", Attributes = "paged-count")]
public class PaginationTagHelper : TagHelper
{
    private readonly IUrlHelperFactory urlHelperFactory;
    private readonly IHtmlGenerator generator;

    private string baseUrl;
    private Dictionary<string, string?> queryParams;
    
    public PaginationTagHelper(IUrlHelperFactory helperFactory, IHtmlGenerator generator)
    {
        urlHelperFactory = helperFactory;
        this.generator = generator;
        queryParams = new Dictionary<string, string?>();
    }
    
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }
    
    public PagedCount PagedCount { get; set; }
    
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
        HttpRequest httpRequest = ViewContext.HttpContext.Request;
        baseUrl = httpRequest.Path;
        foreach (KeyValuePair<string, StringValues> queryString in httpRequest.Query)
        {
            queryParams.Add(queryString.Key, queryString.Value);
        }

        TagBuilder pageControls = new("ul");
        pageControls.AddCssClass(PaginationClass);
        
        //Add previous page link
        pageControls.InnerHtml.AppendHtml(CreatePageLink(PagedCount.CurrentPage - 1, "<span aria-hidden=\"true\">&laquo;</span>", true, "Previous Page"));

        //Create links for each page
        for (int i = 1; i <= PagedCount.TotalPages; i++)
            pageControls.InnerHtml.AppendHtml(CreatePageLink(i, i.ToString(), false, $"Page {i}"));
        
        //Add next page link
        pageControls.InnerHtml.AppendHtml(CreatePageLink(PagedCount.CurrentPage + 1, "<span aria-hidden=\"true\">&raquo;</span>", true, "Next Page"));
        
        //Main pagination nav
        TagBuilder navItem = new("nav");
        navItem.InnerHtml.AppendHtml(pageControls);
        output.Content.AppendHtml(navItem);
        
        //Sizes control
        TagBuilder sizeHolder = new("div");
        sizeHolder.AddCssClass("dropdown");
        sizeHolder.AddCssClass("ms-auto");

        TagBuilder sizesButton = new("button");
        sizesButton.AddCssClass("btn btn-secondary dropdown-toggle");
        sizesButton.Attributes.Add("type", "button");
        sizesButton.Attributes.Add("data-bs-toggle", "dropdown");
        sizesButton.Attributes.Add("aria-expanded", "false");

        sizesButton.InnerHtml.AppendHtml(PagedCount.PageSize.ToString());

        sizeHolder.InnerHtml.AppendHtml(sizesButton);
        
        //Sizes Options
        TagBuilder sizesList = new("ul");
        sizesList.AddCssClass("dropdown-menu");
        sizesList.AddCssClass("ms-auto");

        foreach (int pageSize in PagedCount.PageSizes)
        {
            TagBuilder sizeOptionItem = new("li");
            TagBuilder sizeOptionLink = new("a");
            sizeOptionLink.AddCssClass("dropdown-item");
            if(pageSize == PagedCount.PageSize)
                sizeOptionLink.AddCssClass("active");

            sizeOptionLink.Attributes["href"] = BuildUrl(new Dictionary<string, string> { { "page", "1" }, { "size", pageSize.ToString() } });

            sizeOptionLink.InnerHtml.AppendHtml(pageSize.ToString());
            sizeOptionItem.InnerHtml.AppendHtml(sizeOptionLink);
            sizesList.InnerHtml.AppendHtml(sizeOptionItem);
        }

        sizeHolder.InnerHtml.AppendHtml(sizesList);
        output.Content.AppendHtml(sizeHolder);
    }

    private TagBuilder CreatePageLink(int page, string content, bool previousOrNext, string ariaLabel)
    {
        TagBuilder liTag = new("li");
        liTag.AddCssClass(PageItemClass);
        
        if (page == PagedCount.CurrentPage && !previousOrNext)
        {
            liTag.AddCssClass(PageItemActiveClass);
            liTag.Attributes.Add("aria-current", "page");
        }
        else if (previousOrNext)
        {
            if(page < 1 || page > PagedCount.TotalPages)
                liTag.AddCssClass("disabled");
        }
        
        TagBuilder tag = new("a");
        tag.AddCssClass(PageLinkClass);

        tag.Attributes["href"] = BuildUrl(new Dictionary<string, string> { { "page", page.ToString() } });
        tag.Attributes["aria-label"] = ariaLabel;
        
        tag.InnerHtml.AppendHtml(content);
        
        liTag.InnerHtml.AppendHtml(tag);
        return liTag;
    }

    private string BuildUrl(Dictionary<string, string> updateParams)
    {
        Dictionary<string, string?> builtQueryParams = new(queryParams);
        foreach (KeyValuePair<string, string> queryString in updateParams)
        {
            builtQueryParams[queryString.Key] = queryString.Value;
        }

        return $"{baseUrl}?{string.Join("&", builtQueryParams.Select(x => $"{x.Key}={x.Value}"))}";
    }
}