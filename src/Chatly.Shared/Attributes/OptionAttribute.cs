namespace Chatly.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class OptionAttribute(string? name = null) : Attribute
{
    internal string? Name { get; } = name;
}
