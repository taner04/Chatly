using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Chatly.Desktop.Abstractions;

namespace Chatly.Desktop.Infrastrucuture;

public sealed class NavigationService(PageService pageService) 
{
    private readonly Stack<Type> _backStack = new();
    private readonly Stack<Type> _forwardStack = new();
    private ContentControl? _pageHost;

    public Type? CurrentPage { get; private set; }

    public void SetNavigationView(INavigationView navigationView)
    {
        ArgumentNullException.ThrowIfNull(navigationView);
        _pageHost = navigationView.GetPageHost();
    }

    public bool NavigateTo(Type pageType) => NavigateToCore(pageType, null);

    public bool NavigateTo(Type pageType, object parameter) =>
        NavigateToCore(pageType, parameter);

    public bool GoBack()
    {
        if (_pageHost is null || _backStack.Count == 0)
        {
            return false;
        }

        var target = _backStack.Pop();

        if (CurrentPage is not null)
        {
            _forwardStack.Push(CurrentPage);
        }

        ShowPage(target);
        return true;
    }

    public bool GoForward()
    {
        if (_pageHost is null || _forwardStack.Count == 0)
        {
            return false;
        }

        var target = _forwardStack.Pop();

        if (CurrentPage is not null)
        {
            _backStack.Push(CurrentPage);
        }

        ShowPage(target);
        return true;
    }

    private bool NavigateToCore(Type pageType, object? parameter)
    {
        if (_pageHost is null || CurrentPage == pageType)
        {
            return false;
        }

        var page = pageService.GetPage(pageType);

        if (parameter is not null)
        {
            if (page is not INavigationParameterAware parameterAware)
            {
                return false;
            }

            parameterAware.SetNavigationParameter(parameter);
        }

        if (CurrentPage is not null)
        {
            _backStack.Push(CurrentPage);
        }

        _forwardStack.Clear();
        
        _pageHost.Content = page;
        CurrentPage = pageType;
        
        return true;
    }

    private void ShowPage(Type pageType)
    {
        _pageHost!.Content = pageService.GetPage(pageType);
        CurrentPage = pageType;
    }
}
