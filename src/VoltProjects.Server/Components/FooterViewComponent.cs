using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VoltProjects.Server.Models;
using VoltProjects.Server.Models.View;
using VoltProjects.Server.Shared;

namespace VoltProjects.Server.Components;

[ViewComponent(Name = "Footer")]
public class FooterViewComponent : ViewComponent
{
    private static string[] handingWithWords = new[]
        { "Using", "Rolling with", "Chilling with", "Hanging out with", "Employing", "Hooked up with", "Lovin", "Consuming time with", "Socializing with", "Not touching grass with" };

    private readonly VoltProjectsConfig config;
    
    public FooterViewComponent(IOptions<VoltProjectsConfig> config)
    {
        this.config = config.Value;
    }
    
    public Task<IViewComponentResult> InvokeAsync()
    {
        return Task.FromResult<IViewComponentResult>(View("Footer", new FooterViewModel
        {
            SocialLinks = config.SocialLinks,
            UsingWording = config.FunnyMode ? GetHandingWithWord() : "Using"
        }));
    }

    private static string GetHandingWithWord()
    {
        int index = Random.Shared.Next(0, handingWithWords.Length);
        return handingWithWords[index];
    }
}