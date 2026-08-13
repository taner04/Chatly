using System;
using Chatly.Desktop.Shared.Abstractions;

namespace Chatly.Desktop.Infrastructure.Navigation;

public interface INavigationService
{
    event EventHandler<NavigatedEventArgs>? Navigated;

    Type? CurrentPage { get; }

    bool CanGoBack { get; }

    bool CanGoForward { get; }

    void SetNavigationView(INavigationView navigationView);

    bool NavigateTo(Type pageType);

    bool NavigateTo(Type pageType, object? parameter);

    bool GoBack();

    bool GoForward();
}
