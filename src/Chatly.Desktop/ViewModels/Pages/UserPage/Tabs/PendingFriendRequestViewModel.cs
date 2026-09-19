using Chatly.Desktop.Services.Api.Clients;
using Chatly.Desktop.Services.Friendships;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

public sealed partial class PendingFriendRequestViewModel(
    FriendRequest friendRequest,
    FriendsApiClient friendsApiClient,
    FriendRequestState friendRequestState,
    FriendshipStateService friendshipStateService,
    IToastService toastService) : ViewModelBase
{
    public Guid Id => friendRequest.Id;

    public string SenderUsername => friendRequest.SenderUsername;

    public string? ProfilePictureUrl => friendRequest.ProfilePictureUrl;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeclineCommand))]
    public partial bool IsPending { get; private set; }

    private bool CanRespond() => !IsPending;

    [RelayCommand(CanExecute = nameof(CanRespond))]
    private async Task AcceptAsync(CancellationToken cancellationToken)
    {
        IsPending = true;

        try
        {
            var result = await friendsApiClient.AcceptFriendRequestAsync(Id, cancellationToken);
            if (result.IsFailure)
            {
                toastService.ShowError(result.Error);
                return;
            }

            friendRequestState.Remove(Id);
            friendRequestState.DecrementPendingCount();
            friendshipStateService.ApplyAccepted(result.Value);
            toastService.ShowSuccess($"Friend request from '{SenderUsername}' accepted.");
        }
        finally
        {
            IsPending = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRespond))]
    private async Task DeclineAsync(CancellationToken cancellationToken)
    {
        IsPending = true;

        try
        {
            var result = await friendsApiClient.RejectFriendRequestAsync(Id, cancellationToken);
            if (result.IsFailure)
            {
                toastService.ShowError(result.Error);
                return;
            }

            friendRequestState.Remove(Id);
            friendRequestState.DecrementPendingCount();
            toastService.ShowSuccess($"Friend request from '{SenderUsername}' declined.");
        }
        finally
        {
            IsPending = false;
        }
    }
}