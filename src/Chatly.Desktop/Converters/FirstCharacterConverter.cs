using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Chatly.Desktop.Converters;

public sealed class FirstCharacterConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string text && !string.IsNullOrWhiteSpace(text)
            ? text.Trim()[..1].ToUpperInvariant()
            : "?";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
