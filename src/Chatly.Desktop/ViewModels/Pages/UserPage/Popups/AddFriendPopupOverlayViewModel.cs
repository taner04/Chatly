using System.Linq;
using Chatly.Contracts.Endpoints.FriendRequests.Requests;
using Chatly.Contracts.Endpoints.Users.Requests;
using Chatly.Contracts.Endpoints.Users.Results;
using Chatly.Contracts.Pagination;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Extensions;
using Chatly.Desktop.Services.Api;
using Chatly.Desktop.ViewModels.Popups;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Popups;

[TransientService]
public sealed partial class AddFriendPopupOverlayViewModel(
    IToastService toastService,
    FriendsWebService friendsWebService,
    UserWebService userWebService) : PopupOverlayViewModel
{
    public override string Title => "Add Friend";

    [ObservableProperty] public partial string UserName { get; set; }
    [ObservableProperty] public partial List<UserSearchResultViewModel> SearchedUsers { get; set; }
    [ObservableProperty] public partial PaginationResult<UserSearchResponse> PaginationResult { get; set; }

    [RelayCommand]
    private async Task SendFriendRequest(UserSearchResultViewModel user)
    {
        var sendFriendRequestResult =
            await friendsWebService.SendFriendRequestAsync(new SendFriendRequestRequest(user.UserId));
        if (sendFriendRequestResult.IsFailure)
        {
            toastService.ShowError(sendFriendRequestResult.Error);
        }
        else
        {
            toastService.ShowSuccess($"Friend request sent to {user.Username}.");
            user.RelationshipStatus = UserRelationshipStatus.OutgoingFriendRequest;
        }
    }

    [RelayCommand]
    private async Task SearchForUsers()
    {
        var searchResult = await userWebService.SearchUsersAsync(new SearchUsersRequest(UserName));
        if (searchResult.IsFailure)
        {
            toastService.ShowError(searchResult.Error);
            SearchedUsers = [];
        }
        else
        {
            PaginationResult = searchResult.Value;
            SearchedUsers =
            [
                .. PaginationResult.Items
                    .Where(user => user.RelationshipStatus != UserRelationshipStatus.Friends)
                    .Select(user => new UserSearchResultViewModel(user))
            ];
        }
    }
}