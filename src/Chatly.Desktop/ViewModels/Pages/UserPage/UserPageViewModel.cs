using Chatly.Desktop.ViewModels.Pages.UserPage.Popups;
using Chatly.Desktop.ViewModels.Popups;
using CommunityToolkit.Mvvm.Input;
using Chatly.Desktop.Models.UserSession;

namespace Chatly.Desktop.ViewModels.Pages.UserPage;

[SingletonService]
public sealed partial class UserPageViewModel(
    UserSessionContext userContext,
    FriendRequestState friendRequestState,
    IPopupService popupService) : PageViewModelBase
{
    public UserSessionContext UserContext { get; } = userContext;

    public FriendRequestState FriendRequestState { get; } = friendRequestState;

    [RelayCommand]
    private async Task AddFriend()
    {
        await popupService.ShowAsync<AddFriendPopupOverlayViewModel>();
    }

    [RelayCommand]
    private async Task OpenCurrentUserProfile()
    {
        await popupService.ShowAsync<UserInfoPopupViewModel>();
    }
}
