using Chatly.Desktop.ViewModels.Pages.UserPage.Popups;
using Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;
using Chatly.Desktop.ViewModels.Popups;

namespace Chatly.Desktop.ViewModels.Pages.UserPage;

[SingletonService]
public sealed partial class UserPageViewModel : PageViewModelBase, IDisposable, IAsyncDisposable
{
    private static readonly Type[] TabTypes =
    [
        typeof(OnlineFriendsTabPageViewModel),
        typeof(AllFriendsTabPageViewModel),
        typeof(PendingFriendRequestsTabPageViewModel)
    ];

    private readonly ILogger<UserPageViewModel> _logger;
    private readonly IPopupService _popupService;

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
        TabNavigation = navigationServiceFactory.Create();
    }

    public UserSessionContext UserContext { get; }

    public FriendRequestState FriendRequestState { get; }

    [ObservableProperty] public partial int SelectedTabIndex { get; set; }

    public INavigationService TabNavigation { get; }

    public override async Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        if (TabNavigation.CurrentPage is null)
        {
            await TabNavigation.NavigateToAsync(TabTypes[SelectedTabIndex], cancellationToken);
        }
    }

    async partial void OnSelectedTabIndexChanged(int value)
    {
        if (value >= 0 && value < TabTypes.Length)
        {
            try
            {
                await TabNavigation.NavigateToAsync(TabTypes[value]);
            }
            catch (Exception exception)
            {
                LogTabNavigationFailed(TabTypes[value], exception);
            }
        }
    }

    public void Dispose() => TabNavigation.Dispose();

    public ValueTask DisposeAsync() => TabNavigation.DisposeAsync();

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