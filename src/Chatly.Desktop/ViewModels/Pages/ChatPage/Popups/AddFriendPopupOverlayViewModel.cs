using Chatly.Contracts.FriendRequests.Requests;
using Chatly.Contracts.Pagination;
using Chatly.Contracts.Users.Requests;
using Chatly.Contracts.Users.Results;
using Chatly.Desktop.Abstractions.Toasts;
using Chatly.Desktop.Extentions;
using Chatly.Desktop.Services.Api;
using Chatly.Desktop.ViewModels.Popups;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage.Popups;

public sealed partial class AddFriendPopupOverlayViewModel(
    IToastService toastService,
    FriendsWebService friendsWebService,
    UserWebService userWebService) : PopupOverlayViewModel
{
    public override string Title => "Add Friend";

    [ObservableProperty] public partial string UserName { get; set;  }
    [ObservableProperty] public partial List<UserSearchResponse> SearchedUsers { get; set; }
    [ObservableProperty] public partial PaginationResult<UserSearchResponse> PaginationResult { get; set; }

    [RelayCommand]
    private async Task SendFriendRequest(UserSearchResponse user)
    {
        var sendFriendRequestResult = await friendsWebService.SendFriendRequestAsync(new SendFriendRequestRequest(user.UserId));
        if (sendFriendRequestResult.IsFailure)
        {
            toastService.ShowError(sendFriendRequestResult.Error.Detail);
        }
        else
        {
            toastService.ShowSuccess($"Friend request sent to {user.Username}.");
        }
    }

    [RelayCommand]
    private async Task SearchForUsers()
    {
        var searchResult = await userWebService.SearchUsersAsync(new SearchUsersRequest(UserName));
        if (searchResult.IsFailure)
        {
            toastService.ShowError("Failed to search for users. Please try again.");
            SearchedUsers = [];
        }
        else
        {
            PaginationResult = searchResult.Value;
            SearchedUsers = [.. PaginationResult.Items];
        }
    }    
}