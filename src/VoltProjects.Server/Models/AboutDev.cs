namespace VoltProjects.Server.Models;

/// <summary>
///     A developer to display on the about page
/// </summary>
public struct AboutDev
{
    /// <summary>
    ///     Name of the dev
    /// </summary>
    public string Name { get; init; }
    
    /// <summary>
    ///     Quick description on what they did
    /// </summary>
    public string Description { get; init; }
    
    /// <summary>
    ///     Dev's social links
    /// </summary>
    public SocialLink[] SocialLinks { get; init; }
}