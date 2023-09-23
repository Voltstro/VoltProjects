namespace VoltProjects.Server.Shared;

/// <summary>
///     Versioning info of VoltProjects
/// </summary>
public static class Versioning
{
    public static string Version => ThisAssembly.AssemblyFileVersion[..5];
    
    public static string GitHashFull => "";

    public static string GitHashSmall => "";
}