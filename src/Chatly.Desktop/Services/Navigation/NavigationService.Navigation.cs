using System;
using System.Collections.Generic;
using Chatly.Desktop.Abstractions.Navigation;
using Chatly.Desktop.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.Services.Navigation;

public sealed partial class NavigationService
{
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

    public bool NavigateTo<T>(object parameter) where T : INavigableViewModel
    {
        ArgumentNullException.ThrowIfNull(parameter);

        var currentEntry = _currentEntry;
        if (currentEntry?.ViewModelType == typeof(T))
        {
            _ = NotifyNavigatedToAsync(currentEntry.ViewModel, parameter);
            return true;
        }

        var view = serviceProvider.GetRequiredService<INavigableView<T>>();
        Navigate(new NavigationEntry<T>(view, parameter));
        return true;
    }

    public bool GoBack()
    {
        return NavigateHistory(_backStack, _forwardStack);
    }

    public bool GoForward()
    {
        return NavigateHistory(_forwardStack, _backStack);
    }

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
        _ = NotifyNavigatedFromAsync(_currentEntry?.ViewModel);
        entry.ShowPage(GetNavigationView());
        _currentEntry = entry;
        Navigated?.Invoke(this, new NavigatedEventArgs(entry.ViewModelType));
        _ = NotifyNavigatedToAsync(entry.ViewModel, entry.Parameter);
    }

    private INavigationView GetNavigationView()
    {
        return _navigationView ??
               throw new InvalidOperationException("A navigation view must be set before navigating.");
    }
}
