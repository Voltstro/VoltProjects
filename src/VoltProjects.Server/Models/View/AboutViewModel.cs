namespace VoltProjects.Server.Models.View;

/// <summary>
///     View model for about page
/// </summary>
public sealed class AboutViewModel
{
    /// <summary>
    ///     All devs to display
    /// </summary>
    public AboutDev[] Developers { get; init; }
    
    /// <summary>
    ///     Projects to display
    /// </summary>
    public OpenSourceProject[] Projects { get; init; }
}