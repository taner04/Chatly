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
        _tabNavigationService.NavigateTo<OnlineFriendsTabPageViewModel>();
        TabNavigation.SelectionChanged += TabNavigation_OnSelectionChanged;
    }

    public UserPageViewModel ViewModel { get; }

    public void SetPage<T>(INavigableView<T> view) where T : INavigableViewModel
    {
        TabPageHost.Content = view;
    }

    private void TabNavigation_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (TabNavigation.SelectedIndex)
        {
            case 0:
                _tabNavigationService.NavigateTo<OnlineFriendsTabPageViewModel>();
                break;
            case 1:
                _tabNavigationService.NavigateTo<AllFriendsTabPageViewModel>();
                break;
            case 2:
                _tabNavigationService.NavigateTo<PendingFriendRequestTabPageViewModel>();
                break;
        }
    }
}