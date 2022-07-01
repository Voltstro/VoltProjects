using System.IO;

namespace VoltProjects.Server.Core.Helper;

/// <summary>
///     Helper for IO methods
/// </summary>
public static class IOHelper
{
    /// <summary>
    ///     Copies a directory (and all of it's files) from once place to another
    /// </summary>
    /// <param name="sourceDir"></param>
    /// <param name="destinationDir"></param>
    /// <param name="recursive"></param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        //Get information about the source directory
        DirectoryInfo dir = new(sourceDir);

        //Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        //Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        //Create the destination directory
        Directory.CreateDirectory(destinationDir);

        //Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        //If recursive and copying subdirectories, recursively call this method
        if (!recursive) 
            return;
        
        foreach (DirectoryInfo subDir in dirs)
        {
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir, true);
        }
    }
}