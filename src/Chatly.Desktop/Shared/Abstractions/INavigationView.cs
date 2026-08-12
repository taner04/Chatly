using Avalonia.Controls;

namespace Chatly.Desktop.Shared.Abstractions;

public interface INavigationView
{
    ContentControl GetPageHost();
}