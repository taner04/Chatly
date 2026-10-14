using System.Collections.ObjectModel;
using Chatly.Desktop.ViewModels.Pages;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using Chatly.Desktop.ViewModels.Pages.UserPage;
using FluentIcons.Common;

namespace Chatly.Desktop.ViewModels.Windows;

[SingletonService]
public sealed partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    public MainWindowViewModel(
        INavigationService navigationService,
        ChatSidebarViewModel chatSidebar)
    {
        Navigation = navigationService;
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

    public INavigationService Navigation { get; }

    public List<NavigationItemViewModel> TopNavigationItems { get; }

    public ObservableCollection<ChatPreviewViewModel> Chats { get; }

    public List<NavigationItemViewModel> FooterNavigationItems { get; }

    public void Dispose()
    {
        Navigation.Navigated -= NavigationService_OnNavigated;
    }

    private void NavigationService_OnNavigated(object? sender, NavigatedEventArgs e)
    {
        var currentViewModelType = e.Page.GetType();

        foreach (var item in TopNavigationItems.Concat(FooterNavigationItems))
        {
            item.IsSelected = item.ViewModelType == currentViewModelType;
        }
    }
}