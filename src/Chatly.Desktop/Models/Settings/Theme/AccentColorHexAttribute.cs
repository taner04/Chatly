namespace Chatly.Desktop.Models.Settings.Theme;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class AccentColorHexAttribute(string hexCode) : Attribute
{
    public string HexCode { get; } = hexCode;
}
