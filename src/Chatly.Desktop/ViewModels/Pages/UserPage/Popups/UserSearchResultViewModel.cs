using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;
using Chatly.Contracts.Features.Users.Endpoints.SearchUsers;
using Chatly.Desktop.Services.Api.Clients;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Popups;

public sealed partial class UserSearchResultViewModel(
    UserSearchResponse response,
    FriendsApiClient friendsApiClient,
    IToastService toastService) : ObservableObject
{
    public Guid UserId { get; } = response.UserId;

    public string Username { get; } = response.Username;

    public string? ProfilePictureUrl { get; } = response.ProfilePictureUrl;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendFriendRequestCommand))]
    public partial UserRelationshipStatus RelationshipStatus { get; set; } = response.RelationshipStatus;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendFriendRequestCommand))]
    public partial bool IsSendingFriendRequest { get; private set; }

    public bool CanSendFriendRequest =>
        RelationshipStatus == UserRelationshipStatus.None && !IsSendingFriendRequest;

    public string RelationshipLabel => RelationshipStatus switch
    {
        UserRelationshipStatus.OutgoingFriendRequest => "Request sent",
        UserRelationshipStatus.IncomingFriendRequest => "Incoming request",
        UserRelationshipStatus.Friends => "Friends",
        _ => string.Empty
    };

    partial void OnRelationshipStatusChanged(UserRelationshipStatus value)
    {
        OnPropertyChanged(nameof(CanSendFriendRequest));
        OnPropertyChanged(nameof(RelationshipLabel));
    }

    partial void OnIsSendingFriendRequestChanged(bool value)
    {
        OnPropertyChanged(nameof(CanSendFriendRequest));
    }

    [RelayCommand(CanExecute = nameof(CanSendFriendRequest))]
    private async Task SendFriendRequestAsync(CancellationToken cancellationToken)
    {
        IsSendingFriendRequest = true;

        try
        {
            var result = await friendsApiClient.SendFriendRequestAsync(
                new SendFriendRequestRequest(UserId),
                cancellationToken);
            if (result.IsFailure)
            {
                toastService.ShowError(result.Error);
                return;
            }

            RelationshipStatus = UserRelationshipStatus.OutgoingFriendRequest;
            toastService.ShowSuccess($"Friend request sent to {Username}.");
        }
        finally
        {
            IsSendingFriendRequest = false;
        }
    }

    internal void CancelPendingRequest()
    {
        SendFriendRequestCommand.Cancel();
    }
}