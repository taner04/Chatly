using System;

namespace Chatly.Desktop.Abstractions;

public interface INavigationView
{
    bool Navigate(Type pageType);
    bool Navigate(Type pageType, object parameter);
}