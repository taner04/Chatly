using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Chatly.Desktop.Converters;

public sealed class ImageUrlConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            Uri uri => uri.AbsoluteUri,
            string text when !string.IsNullOrWhiteSpace(text) => text,
            _ => null
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
