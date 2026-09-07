using Chatly.Desktop.ViewModels.Pages.UserPage.Popups;
using Chatly.Desktop.ViewModels.Popups;
using CommunityToolkit.Mvvm.Input;
using UserSessionContext = Chatly.Desktop.Models.UserSession.UserSessionContext;

namespace Chatly.Desktop.ViewModels.Pages.UserPage;

[SingletonService]
public sealed partial class UserPageViewModel(
    UserSessionContext userContext,
    IPopupService popupService) : PageViewModelBase
{
    public UserSessionContext UserContext { get; } = userContext;

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