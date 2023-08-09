using System;

namespace VoltProjects.Server.Shared;

public static class WordingUtils
{
    private static string[] handingWithWords = new[]
        { "Using", "Rolling with", "Chilling with", "Hanging out with", "Employing", "Hooked up with", "Lovin", "Consuming time with", "Socializing with", "Not touching grass with", "Gay with" };

    public static string GetHandingWithWord()
    {
        int index = Random.Shared.Next(0, handingWithWords.Length);
        return handingWithWords[index];
    }
}