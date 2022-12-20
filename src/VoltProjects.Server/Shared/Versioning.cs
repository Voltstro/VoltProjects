namespace VoltProjects.Server.Shared;

/// <summary>
///     Versioning info of VoltProjects
/// </summary>
public static class Versioning
{
    public static string Version => ThisAssembly.AssemblyFileVersion[..5];
    
    public static string GitHashFull => ThisAssembly.GitCommitId;

    public static string GitHashSmall => GitHashFull[..7];
}