using System.Collections.ObjectModel;
using System.Linq;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Extensions;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Services.Api;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

[SingletonService]
public sealed partial class PendingFriendRequestTabPageViewModel(
    IToastService toastService,
    FriendsWebService friendsWebService,
    UserSessionContext userSessionContext) : PageViewModelBase, INavigationAware
{
    public ObservableCollection<FriendRequest> FriendRequests => userSessionContext.FriendRequests;

    public async Task OnNavigatedToAsync()
    {
        var pendingFriendRequests = await friendsWebService.GetFriendRequestsAsync(1, 10);
        if (pendingFriendRequests.IsFailure)
        {
            toastService.ShowError(pendingFriendRequests.Error);
            userSessionContext.SetFriendRequests([]);
        }
        else
        {
            userSessionContext.SetFriendRequests(
                pendingFriendRequests.Value.Items.Select(FriendRequestMapper.Map));
        }
    }

    public Task OnNavigatedFromAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task AcceptFriendRequest(FriendRequest friendRequest)
    {
        var acceptResult = await friendsWebService.AcceptFriendRequestAsync(friendRequest.Id);
        if (acceptResult.IsFailure)
        {
            toastService.ShowError(acceptResult.Error);
        }
        else
        {
            userSessionContext.RemoveFriendRequest(friendRequest.Id);
            userSessionContext.AddFriend(FriendMapper.Map(acceptResult.Value));
            userSessionContext.AddDirectChat(DirectChatMapper.Map(acceptResult.Value));
            toastService.ShowSuccess($"Friend request from '{friendRequest.SenderUsername}' accepted.");
        }
    }

    [RelayCommand]
    private async Task DeclineFriendRequest(FriendRequest friendRequest)
    {
        var declineResult = await friendsWebService.RejectFriendRequestAsync(friendRequest.Id);
        if (declineResult.IsFailure)
        {
            toastService.ShowError(declineResult.Error);
        }
        else
        {
            userSessionContext.RemoveFriendRequest(friendRequest.Id);
            toastService.ShowSuccess($"Friend request from '{friendRequest.SenderUsername}' declined.");
        }
    }
}