namespace VoltProjects.DocsBuilder.Core;

public class DocsBuilderAction
{
    /// <summary>
    ///     What program to use
    /// </summary>
    public string Program { get; set; } = string.Empty;

    /// <summary>
    ///     Arguments to launch the program with
    /// </summary>
    public string Arguments { get; set; } = string.Empty;

    /// <summary>
    ///     Set environmental variables
    /// </summary>
    public Dictionary<string, string>? EnvironmentalVariables { get; set; }
    
    /// <summary>
    ///     Maps two environmental variables together
    /// </summary>
    public Dictionary<string, string>? EnvironmentalVariablesMapping { get; set; }
}