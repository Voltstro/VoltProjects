namespace VoltProjects.Builder;

/// <summary>
///     Marks a <see cref="Builder"/>'s name
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class BuilderNameAttribute : Attribute
{
    /// <summary>
    ///     The name of the builder
    /// </summary>
    public string Name { get; set; }
}