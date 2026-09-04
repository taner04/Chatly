namespace Chatly.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class OptionAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
}
