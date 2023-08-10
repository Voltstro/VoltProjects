namespace VoltProjects.Server.Models;

/// <summary>
///     A social icon that can be displayed
/// </summary>
public class SocialLink
{
    /// <summary>
    /// Name of the Icon
    /// </summary>
    public string Name { get; init; }
    
    /// <summary>
    ///     Icon to use for displaying
    /// </summary>
    public string Icon { get; init; }
    
    /// <summary>
    ///     Href (aka the link to go to)
    /// </summary>
    public string Href { get; init; }
}