namespace VoltProjects.Builder.Builders;

/// <summary>
///     Marks a <see cref="IBuilder"/>'s name
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class BuilderNameAttribute : Attribute
{
    /// <summary>
    ///     The name of the builder
    /// </summary>
    public string Name { get; set; }
}