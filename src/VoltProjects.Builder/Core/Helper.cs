using System.Security.Cryptography;

namespace VoltProjects.Builder.Core;

public static class Helper
{
    public static string GetFileHash(Stream stream)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(stream);

        stream.Position = 0;
        return Convert.ToBase64String(hash);
    }
}