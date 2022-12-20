using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VoltProjects.Server.Shared.Helpers;

//Credit:
//https://levelup.gitconnected.com/using-asp-net-mvc-to-specify-which-element-in-a-navigation-bar-is-active-9c3dac154f9c
public static class MvcHelper
{
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
}