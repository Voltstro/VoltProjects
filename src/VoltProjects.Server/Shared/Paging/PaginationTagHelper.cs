using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace VoltProjects.Server.Shared.Paging;

[HtmlTargetElement("div", Attributes = "paged-count")]
public class PaginationTagHelper : TagHelper
{
    private IUrlHelperFactory urlHelperFactory;
    private IHtmlGenerator generator;
    
    public PaginationTagHelper(IUrlHelperFactory helperFactory, IHtmlGenerator generator)
    {
        urlHelperFactory = helperFactory;
        this.generator = generator;
    }
    
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }
    
    public PagedCount PagedCount { get; set; }
    
    public string BaseUrl { get; set; }
    
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
        
        TagBuilder pageControls = new("ul");
        pageControls.AddCssClass(PaginationClass);
        
        //Add previous page link
        pageControls.InnerHtml.AppendHtml(CreatePageLink(PagedCount.CurrentPage - 1, "<span aria-hidden=\"true\">&laquo;</span>", true, currentParameters, "Previous Page"));

        //Create links for each page
        for (int i = 1; i <= PagedCount.TotalPages; i++)
            pageControls.InnerHtml.AppendHtml(CreatePageLink(i, i.ToString(), false, currentParameters, $"Page {i}"));
        
        //Add next page link
        pageControls.InnerHtml.AppendHtml(CreatePageLink(PagedCount.CurrentPage + 1, "<span aria-hidden=\"true\">&raquo;</span>", true, currentParameters, "Next Page"));
        
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

        string baseSizeUrl = $"{BaseUrl}?page={PagedCount.CurrentPage}&size=";
        
        foreach (int pageSize in PagedCount.PageSizes)
        {
            TagBuilder sizeOptionItem = new("li");
            TagBuilder sizeOptionLink = new("a");
            sizeOptionLink.AddCssClass("dropdown-item");
            if(pageSize == PagedCount.PageSize)
                sizeOptionLink.AddCssClass("active");

            sizeOptionLink.Attributes["href"] = baseSizeUrl + pageSize;

            sizeOptionLink.InnerHtml.AppendHtml(pageSize.ToString());
            sizeOptionItem.InnerHtml.AppendHtml(sizeOptionLink);
            sizesList.InnerHtml.AppendHtml(sizeOptionItem);
        }

        sizeHolder.InnerHtml.AppendHtml(sizesList);
        output.Content.AppendHtml(sizeHolder);
    }

    private TagBuilder CreatePageLink(int page, string content, bool previousOrNext, string urlParameters, string ariaLabel)
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

        string href = $"{BaseUrl}?page={page}";
        if (urlParameters != string.Empty)
            href += $"&{urlParameters}";

        tag.Attributes["href"] = href;
        tag.Attributes["aria-label"] = ariaLabel;
        
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