using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Chatly.Desktop.Abstractions.Navigation;
using Chatly.Desktop.ViewModels.Pages;
using Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

namespace Chatly.Desktop.Views.Pages;

public partial class UserPage
    : UserControl, INavigableView<UserPageViewModel>, INavigationView
{
    private INavigationService? _tabNavigationService;

    public UserPage(
        UserPageViewModel viewModel,
        INavigationServiceFactory navigationServiceFactory)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        _tabNavigationService = navigationServiceFactory.Create();
        _tabNavigationService.SetNavigationView(this);
        _tabNavigationService.NavigateTo<OnlineFriendsTabPageViewModel>();
    }

    public UserPageViewModel ViewModel { get; }

    public void SetPage<T>(INavigableView<T> view) where T : INavigableViewModel
    {
        TabPageHost.Content = view;
    }

    private void TabNavigation_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_tabNavigationService is null)
        {
            return;
        }

        switch (((TabStrip)sender!).SelectedIndex)
        {
            case 0:
                _tabNavigationService.NavigateTo<OnlineFriendsTabPageViewModel>();
                break;
            case 1:
                _tabNavigationService.NavigateTo<AllFriendTabPageViewModel>();
                break;
            case 2:
                _tabNavigationService.NavigateTo<PendingFriendRequestTabPageViewModel>();
                break;
        }
    }
}
