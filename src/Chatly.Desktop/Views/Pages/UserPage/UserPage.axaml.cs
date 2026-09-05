using Chatly.Desktop.ViewModels.Pages.UserPage;
using Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

namespace Chatly.Desktop.Views.Pages.UserPage;

[SingletonService(typeof(INavigableView<UserPageViewModel>))]
public partial class UserPage
    : UserControl, INavigableView<UserPageViewModel>, INavigationView
{
    private readonly INavigationService _tabNavigationService;

    public UserPage(
        UserPageViewModel viewModel,
        INavigationServiceFactory navigationServiceFactory)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        _tabNavigationService = navigationServiceFactory.Create();
        _tabNavigationService.SetNavigationView(this);
        Loaded += UserPage_OnLoaded;
        TabNavigation.SelectionChanged += TabNavigation_OnSelectionChanged;
    }

    public UserPageViewModel ViewModel { get; }

    public void SetPage<T>(INavigableView<T> view) where T : INavigableViewModel
    {
        TabPageHost.Content = view;
    }

    private async void UserPage_OnLoaded(object? sender, EventArgs e)
    {
        if (TabPageHost.Content is null)
        {
            await _tabNavigationService.NavigateToAsync<OnlineFriendsTabPageViewModel>();
        }
    }

    private async void TabNavigation_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (TabNavigation.SelectedIndex)
        {
            case 0:
                await _tabNavigationService.NavigateToAsync<OnlineFriendsTabPageViewModel>();
                break;
            case 1:
                await _tabNavigationService.NavigateToAsync<AllFriendsTabPageViewModel>();
                break;
            case 2:
                await _tabNavigationService.NavigateToAsync<PendingFriendRequestTabPageViewModel>();
                break;
        }
    }
}