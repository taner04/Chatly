using System.Threading.Tasks;
using Chatly.Desktop.Models;
using Chatly.Desktop.Abstractions.Popups;
using Chatly.Desktop.ViewModels.Pages.ChatPage.Popups;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Pages;

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
}
