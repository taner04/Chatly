using System;
using System.Collections.Generic;
using Chatly.Desktop.Abstractions.Navigation;
using Chatly.Desktop.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.Services.Navigation;

public sealed class NavigationService(IServiceProvider serviceProvider) : INavigationService
{
    private readonly Stack<NavigationEntry> _backStack = new();
    private readonly Stack<NavigationEntry> _forwardStack = new();
    private NavigationEntry? _currentEntry;
    private INavigationView? _navigationView;

    public event EventHandler<NavigatedEventArgs>? Navigated;

    public Type? CurrentViewModelType => _currentEntry?.ViewModelType;

    public bool CanGoBack => _backStack.Count > 0;

    public bool CanGoForward => _forwardStack.Count > 0;

    public void SetNavigationView(INavigationView navigationView)
    {
        _navigationView = navigationView ?? throw new ArgumentNullException(nameof(navigationView));
    }

    public bool NavigateTo<T>() where T : INavigableViewModel
    {
        if (_currentEntry?.ViewModelType == typeof(T))
        {
            return false;
        }

        var view = serviceProvider.GetRequiredService<INavigableView<T>>();
        Navigate(new NavigationEntry<T>(view));
        return true;
    }

    public bool GoBack() => NavigateHistory(_backStack, _forwardStack);

    public bool GoForward() => NavigateHistory(_forwardStack, _backStack);

    private bool NavigateHistory(
        Stack<NavigationEntry> source,
        Stack<NavigationEntry> destination)
    {
        if (source.Count == 0)
        {
            return false;
        }

        var target = source.Pop();

        if (_currentEntry is not null)
        {
            destination.Push(_currentEntry);
        }

        Show(target);
        return true;
    }

    private void Navigate(NavigationEntry target)
    {
        if (_currentEntry is not null)
        {
            _backStack.Push(_currentEntry);
        }

        _forwardStack.Clear();
        Show(target);
    }

    private void Show(NavigationEntry entry)
    {
        entry.ShowPage(GetNavigationView());
        _currentEntry = entry;
        Navigated?.Invoke(this, new NavigatedEventArgs(entry.ViewModelType));
    }

    private INavigationView GetNavigationView()
    {
        return _navigationView ?? throw new InvalidOperationException("A navigation view must be set before navigating.");
    }
}
