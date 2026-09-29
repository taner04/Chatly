using System.Collections.ObjectModel;
using Chatly.Desktop.ViewModels.Pages;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using Chatly.Desktop.ViewModels.Pages.UserPage;
using FluentIcons.Common;

namespace Chatly.Desktop.ViewModels.Windows;

[SingletonService]
public sealed partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly INavigationService _navigationService;

    public MainWindowViewModel(
        INavigationService navigationService,
        ChatSidebarViewModel chatSidebar)
    {
        _navigationService = navigationService;
        TopNavigationItems =
        [
            NavigationItemViewModel.Create<UserPageViewModel>("User", Symbol.People, navigationService),
            NavigationItemViewModel.Create<ChatPageViewModel>("Chats", Symbol.Chat, navigationService)
        ];
        Chats = chatSidebar.UnreadChats;
        FooterNavigationItems =
        [
            NavigationItemViewModel.Create<SettingsPageViewModel>("Settings", Symbol.Settings, navigationService)
        ];

        navigationService.Navigated += NavigationService_OnNavigated;
    }

    public List<NavigationItemViewModel> TopNavigationItems { get; }

    public ObservableCollection<ChatPreviewViewModel> Chats { get; }

    public List<NavigationItemViewModel> FooterNavigationItems { get; }

    [ObservableProperty] public partial INavigableViewModel? CurrentPage { get; private set; }

    public void Dispose()
    {
        _navigationService.Navigated -= NavigationService_OnNavigated;
    }

    private void NavigationService_OnNavigated(object? sender, NavigatedEventArgs e)
    {
        CurrentPage = e.Page;
        var currentViewModelType = e.Page.GetType();

        foreach (var item in TopNavigationItems.Concat(FooterNavigationItems))
        {
            item.IsSelected = item.ViewModelType == currentViewModelType;
        }
    }
}