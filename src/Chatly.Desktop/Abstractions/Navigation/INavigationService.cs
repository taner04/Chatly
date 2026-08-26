using System;
using Chatly.Desktop.Models;

namespace Chatly.Desktop.Abstractions.Navigation;

public interface INavigationService
{
    Type? CurrentViewModelType { get; }

    bool CanGoBack { get; }

    bool CanGoForward { get; }
    event EventHandler<NavigatedEventArgs>? Navigated;

    void SetNavigationView(INavigationView navigationView);

    bool NavigateTo<T>() where T : INavigableViewModel;

    bool GoBack();

    bool GoForward();
}
