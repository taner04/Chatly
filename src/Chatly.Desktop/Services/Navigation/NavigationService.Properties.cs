using System;
using Chatly.Desktop.Abstractions.Navigation;
using Chatly.Desktop.Models;

namespace Chatly.Desktop.Services.Navigation;

public sealed partial class NavigationService
{
    public event EventHandler<NavigatedEventArgs>? Navigated;

    public Type? CurrentViewModelType => _currentEntry?.ViewModelType;

    public bool CanGoBack => _backStack.Count > 0;

    public bool CanGoForward => _forwardStack.Count > 0;

    public void SetNavigationView(INavigationView navigationView)
    {
        _navigationView = navigationView ?? throw new ArgumentNullException(nameof(navigationView));
    }
}
