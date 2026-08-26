using System;
using System.Collections.Generic;
using Chatly.Desktop.Abstractions.Navigation;
using Chatly.Desktop.Models;
using Chatly.Desktop.ViewModels.Pages;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;

namespace Chatly.Desktop.ViewModels.Windows;

public sealed partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly INavigationService _navigationService;

    public MainWindowViewModel(
        INavigationService navigationService,
        UserSessionContext userContext)
    {
        _navigationService = navigationService;
        UserContext = userContext;
        var chats = NavigationItemViewModel.Create<ChatPageViewModel>(
            "Chats",
            Symbol.Chat,
            navigationService);
        chats.IsSelected = true;
        NavigationItems =
        [
            chats
        ];
        FooterNavigationItems =
        [
            NavigationItemViewModel.Create<UserInfoPageViewModel>("User", Symbol.People, navigationService)
        ];

        navigationService.Navigated += NavigationService_OnNavigated;
    }

    public UserSessionContext UserContext { get; }

    public List<NavigationItemViewModel> NavigationItems { get; }

    public List<NavigationItemViewModel> FooterNavigationItems { get; }

    [ObservableProperty] public partial NavigationItemViewModel CurrentNavigationItem { get; set; } = null!;

    public void Dispose()
    {
        _navigationService.Navigated -= NavigationService_OnNavigated;
    }

    private void NavigationService_OnNavigated(object? sender, NavigatedEventArgs e)
    {
        var currentViewModelType = e.ViewModelType;

        foreach (var item in NavigationItems)
        {
            item.IsSelected = item.ViewModelType == currentViewModelType;
        }

        foreach (var item in FooterNavigationItems)
        {
            item.IsSelected = item.ViewModelType == currentViewModelType;
        }
    }
}
