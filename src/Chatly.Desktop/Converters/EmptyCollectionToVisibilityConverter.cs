using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Chatly.Desktop.Converters;

public sealed class EmptyCollectionToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is ICollection collection && collection.Count == 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}