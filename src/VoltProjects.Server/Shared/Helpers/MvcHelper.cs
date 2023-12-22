using System;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace VoltProjects.Server.Shared.Helpers;

public static class MvcHelper
{
    //See theme.ts in client side too
    private const string CookieThemeName = "vp-theme";
    private static readonly string[] ValidThemes = { "dark", "light" };
    
    //Credit:
    //https://levelup.gitconnected.com/using-asp-net-mvc-to-specify-which-element-in-a-navigation-bar-is-active-9c3dac154f9c
    
    /// <summary>
    ///     Can add a CSS class if the defined controller(s) and action(s) are currently being used
    /// </summary>
    /// <param name="htmlHelper"></param>
    /// <param name="controllers"></param>
    /// <param name="actions"></param>
    /// <param name="cssClass"></param>
    /// <returns></returns>
    public static string ActiveClass(this IHtmlHelper htmlHelper, string? controllers = null, string? actions = null,
        string cssClass = "active")
    {
        string? currentController = htmlHelper.ViewContext.RouteData.Values["controller"] as string;
        string? currentAction = htmlHelper.ViewContext.RouteData.Values["action"] as string;

        string[] acceptedControllers = (controllers ?? currentController ?? "").Split(',');
        string[] acceptedActions = (actions ?? currentAction ?? "").Split(',');
        
        return acceptedControllers.Contains(currentController) && acceptedActions.Contains(currentAction)
            ? cssClass
            : "";
    }

    /// <summary>
    ///     Gets a user's theme, or dark if they haven't set one
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static string GetUserTheme(HttpRequest request)
    {
        if (request.Cookies.TryGetValue(CookieThemeName, out string? theme))
        {
            //Odd cookie? Shouldn't really happen
            if (!ValidThemes.Contains(theme))
                theme = "dark";
        }
        else
        {
            theme = "dark";
        }

        return theme;
    }
    
    public static string Attr(this IHtmlHelper helper, string name, string value, Func<bool>? condition = null)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var render = condition != null ? condition() : true;

        return render ? $"{name}={HttpUtility.HtmlAttributeEncode(value)}" : string.Empty;
    }
}