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
        UserSessionContext userContext,
        ChatSidebarViewModel chatSidebar)
    {
        _navigationService = navigationService;
        UserContext = userContext;
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

    public UserSessionContext UserContext { get; }

    public List<NavigationItemViewModel> TopNavigationItems { get; }

    [ObservableProperty] public partial ObservableCollection<ChatPreviewViewModel> Chats { get; set; }

    public List<NavigationItemViewModel> FooterNavigationItems { get; }

    [ObservableProperty] public partial NavigationItemViewModel CurrentNavigationItem { get; set; } = null!;

    public void Dispose()
    {
        _navigationService.Navigated -= NavigationService_OnNavigated;
    }

    private void NavigationService_OnNavigated(object? sender, NavigatedEventArgs e)
    {
        var currentViewModelType = e.ViewModelType;

        foreach (var item in TopNavigationItems)
        {
            item.IsSelected = item.ViewModelType == currentViewModelType;
        }

        foreach (var item in FooterNavigationItems)
        {
            item.IsSelected = item.ViewModelType == currentViewModelType;
        }
    }
}