using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Chatly.Desktop.Abstractions.Navigation;
using Chatly.Desktop.Models;
namespace Chatly.Desktop.Services.Navigation;

public sealed class NavigationService(PageService pageService) : INavigationService
{
    private readonly Stack<NavigationEntry> _backStack = new();
    private readonly Stack<NavigationEntry> _forwardStack = new();
    private ContentControl? _pageHost;
    private INavigablePage? _currentPageInstance;
    private NavigationEntry? _currentEntry;

    public event EventHandler<NavigatedEventArgs>? Navigated;

    public Type? CurrentPage => _currentEntry?.PageType;

    public bool CanGoBack => _backStack.Count > 0;

    public bool CanGoForward => _forwardStack.Count > 0;

    public void SetNavigationView(INavigationView navigationView)
    {
        ArgumentNullException.ThrowIfNull(navigationView);
        _pageHost = navigationView.GetPageHost();
    }

    public bool NavigateTo(Type pageType) => NavigateTo(pageType, null);

    public bool NavigateTo(Type pageType, object? parameter)
    {
        EnsureNavigationViewIsSet();
        ArgumentNullException.ThrowIfNull(pageType);

        var entry = new NavigationEntry(pageType, parameter);
        if (_currentEntry == entry)
        {
            return false;
        }

        var page = ResolvePage(entry);

        if (_currentEntry is not null)
        {
            _backStack.Push(_currentEntry);
        }

        _forwardStack.Clear();
        ShowPage(entry, page);
        return true;
    }

    public bool GoBack()
    {
        EnsureNavigationViewIsSet();
        if (_backStack.Count == 0)
        {
            return false;
        }

        var target = _backStack.Pop();
        var page = ResolvePage(target);

        if (_currentEntry is not null)
        {
            _forwardStack.Push(_currentEntry);
        }

        ShowPage(target, page);
        return true;
    }

    public bool GoForward()
    {
        EnsureNavigationViewIsSet();
        if (_forwardStack.Count == 0)
        {
            return false;
        }

        var target = _forwardStack.Pop();
        var page = ResolvePage(target);

        if (_currentEntry is not null)
        {
            _backStack.Push(_currentEntry);
        }

        ShowPage(target, page);
        return true;
    }

    private INavigablePage ResolvePage(NavigationEntry entry)
    {
        var page = pageService.GetPage(entry.PageType);
        if (entry.Parameter is not null && page is not INavigationParameterAware)
        {
            throw new InvalidOperationException($"{entry.PageType.FullName} must implement {nameof(INavigationParameterAware)} to receive a navigation parameter.");
        }

        return page;
    }

    private void ShowPage(NavigationEntry entry, INavigablePage page)
    {
        if (_currentPageInstance is INavigationAware currentAware)
        {
            currentAware.OnNavigatedFrom();
        }

        if (entry.Parameter is not null)
        {
            ((INavigationParameterAware)page).SetNavigationParameter(entry.Parameter);
        }

        _pageHost!.Content = page;
        _currentPageInstance = page;
        _currentEntry = entry;

        if (page is INavigationAware targetAware)
        {
            targetAware.OnNavigatedTo();
        }

        Navigated?.Invoke(this, new NavigatedEventArgs(entry.PageType, entry.Parameter));
    }

    private void EnsureNavigationViewIsSet()
    {
        if (_pageHost is null)
        {
            throw new InvalidOperationException("A navigation view must be set before navigating.");
        }
    }

    private sealed record NavigationEntry(Type PageType, object? Parameter);
}
