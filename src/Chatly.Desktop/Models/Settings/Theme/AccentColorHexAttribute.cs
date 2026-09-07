namespace Chatly.Desktop.Models.Settings.Theme;

[AttributeUsage(AttributeTargets.Field)]
public sealed class AccentColorHexAttribute(string hexCode) : Attribute
{
    public string HexCode { get; } = hexCode;
}