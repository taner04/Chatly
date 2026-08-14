using Avalonia.Controls;

namespace Chatly.Desktop.Abstractions.Navigation;

public interface INavigationView
{
    ContentControl GetPageHost();
}
