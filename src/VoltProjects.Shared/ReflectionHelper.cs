using System.Reflection;

namespace VoltProjects.Shared;

public static class ReflectionHelper
{
    /// <summary>
    ///     Gets all <see cref="Type" />s that is a sub class of <see cref="T" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IReadOnlyList<Type> GetInheritedTypes<T>() where T : class
    {
        return Assembly.GetAssembly(typeof(T))
            .GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)))
            .ToList();
    }
}