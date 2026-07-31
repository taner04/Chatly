using Avalonia.Controls;

namespace Chatly.Desktop.Abstractions;

public interface INavigationView
{
    ContentControl GetPageHost();
}