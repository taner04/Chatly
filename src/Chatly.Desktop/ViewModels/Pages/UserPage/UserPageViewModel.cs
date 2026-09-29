using Chatly.Desktop.ViewModels.Pages.UserPage.Popups;
using Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;
using Chatly.Desktop.ViewModels.Popups;

namespace Chatly.Desktop.ViewModels.Pages.UserPage;

[SingletonService]
public sealed partial class UserPageViewModel : PageViewModelBase
{
    private static readonly Type[] TabTypes =
    [
        typeof(OnlineFriendsTabPageViewModel),
        typeof(AllFriendsTabPageViewModel),
        typeof(PendingFriendRequestsTabPageViewModel)
    ];

    private readonly ILogger<UserPageViewModel> _logger;
    private readonly IPopupService _popupService;
    private readonly INavigationService _tabNavigation;

    public UserPageViewModel(
        UserSessionContext userContext,
        FriendRequestState friendRequestState,
        IPopupService popupService,
        INavigationServiceFactory navigationServiceFactory,
        ILogger<UserPageViewModel> logger)
    {
        UserContext = userContext;
        FriendRequestState = friendRequestState;
        _popupService = popupService;
        _logger = logger;
        _tabNavigation = navigationServiceFactory.Create();
        _tabNavigation.Navigated += TabNavigation_OnNavigated;
    }

    public UserSessionContext UserContext { get; }

    public FriendRequestState FriendRequestState { get; }

    [ObservableProperty] public partial int SelectedTabIndex { get; set; }

    [ObservableProperty] public partial INavigableViewModel? CurrentTab { get; private set; }

    public override async Task OnNavigatedToAsync(
        object? parameter,
        CancellationToken cancellationToken)
    {
        if (CurrentTab is null)
        {
            await _tabNavigation.NavigateToAsync(TabTypes[SelectedTabIndex], cancellationToken);
        }
    }

    async partial void OnSelectedTabIndexChanged(int value)
    {
        if (value >= 0 && value < TabTypes.Length)
        {
            try
            {
                await _tabNavigation.NavigateToAsync(TabTypes[value]);
            }
            catch (Exception exception)
            {
                LogTabNavigationFailed(TabTypes[value], exception);
            }
        }
    }

    private void TabNavigation_OnNavigated(object? sender, NavigatedEventArgs e)
    {
        CurrentTab = e.Page;
    }

    [LoggerMessage(LogLevel.Error, "Navigating to tab {TabType} failed.")]
    private partial void LogTabNavigationFailed(Type tabType, Exception exception);

    [RelayCommand]
    private async Task AddFriend()
    {
        await _popupService.ShowAsync<AddFriendPopupOverlayViewModel>();
    }

    [RelayCommand]
    private async Task OpenCurrentUserProfile()
    {
        await _popupService.ShowAsync<UserInfoPopupViewModel>();
    }
}