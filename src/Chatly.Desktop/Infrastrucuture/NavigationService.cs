using System;
using System.Collections.Generic;
using System.Linq;
using Chatly.Desktop.Abstractions;

namespace Chatly.Desktop.Infrastrucuture;

public sealed class NavigationService(
    IEnumerable<INavigationParameterAware> navigationParameterAwaresPages,
    INavigationView navigationControl)
{
    private readonly Stack<Type> _backStack = new();
    private readonly Stack<Type> _forwardStack = new();

    private bool CanGoBack => _backStack.Count > 0;
    private bool CanGoForward => _forwardStack.Count > 0;

    public Type? CurrentPage { get; set; }

    public bool NavigateTo(Type pageType)
    {
        if (CurrentPage == pageType)
        {
            return false;
        }

        if (CurrentPage != null)
        {
            _backStack.Push(CurrentPage);
        }

        _forwardStack.Clear();

        return navigationControl.Navigate(pageType);
    }

    public bool NavigateTo(Type pageType, object parameter)
    {
        var page = navigationParameterAwaresPages.FirstOrDefault(p => p.GetType() == pageType);
        if (page is null)
        {
            return false;
        }

        page.SetNavigationParameter(parameter);

        if (CurrentPage == pageType)
        {
            return true;
        }

        if (CurrentPage != null)
        {
            _backStack.Push(CurrentPage);
        }

        _forwardStack.Clear();
        return NavigateTo(pageType);
    }

    public new bool GoBack()
    {
        if (!CanGoBack)
        {
            return false;
        }

        var target = _backStack.Pop();

        if (CurrentPage != null)
        {
            _forwardStack.Push(CurrentPage);
        }

        return navigationControl.Navigate(target);
    }

    public bool GoForward()
    {
        if (!CanGoForward)
        {
            return false;
        }

        var target = _forwardStack.Pop();

        if (CurrentPage != null)
        {
            _backStack.Push(CurrentPage);
        }

        return navigationControl.Navigate(target);
    }
}