using Chatly.Desktop.Models.Settings.Theme;

namespace Chatly.Desktop.Extensions;

internal static class EnumExtension
{
    extension(AccentColor accentColor)
    {
        public string GetHexCode()
        {
            var type = typeof(AccentColor);
            var memberInfo = type.GetMember(accentColor.ToString());
            if (memberInfo.Length <= 0)
            {
                throw new ArgumentException($"No member found for {accentColor}", nameof(accentColor));
            }

            var attributes = memberInfo[0].GetCustomAttributes(typeof(AccentColorHexAttribute), false);
            if (attributes.Length > 0)
            {
                return ((AccentColorHexAttribute)attributes[0]).HexCode;
            }

            throw new ArgumentException($"No hex code found for {accentColor}", nameof(accentColor));
        }
    }
}