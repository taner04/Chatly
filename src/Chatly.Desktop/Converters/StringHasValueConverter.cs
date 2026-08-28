using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Chatly.Desktop.Converters;

public sealed class StringHasValueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hasValue = value switch
        {
            Uri => true,
            string text => !string.IsNullOrWhiteSpace(text),
            _ => false
        };
        return string.Equals(parameter as string, "Invert", StringComparison.Ordinal) ? !hasValue : hasValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
