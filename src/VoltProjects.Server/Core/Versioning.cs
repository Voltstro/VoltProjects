namespace VoltProjects.Server.Core;

public static class Versioning
{
    public static string Version => ThisAssembly.AssemblyVersion[..5];
    
    public static string GitHashFull => ThisAssembly.GitCommitId;

    public static string GitHashSmall => GitHashFull[..7];
}