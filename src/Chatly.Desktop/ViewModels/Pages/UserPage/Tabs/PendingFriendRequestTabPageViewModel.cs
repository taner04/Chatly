using System.Collections.ObjectModel;
using System.Linq;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Extensions;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Models.UserSession;
using Chatly.Desktop.Services.Api;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

[SingletonService]
public sealed partial class PendingFriendRequestTabPageViewModel(
    IToastService toastService,
    FriendsApiClient friendsApiClient,
    FriendRequestState friendRequestState,
    FriendState friendState,
    DirectChatState directChatState) : PageViewModelBase
{
    private const int FriendRequestPageSize = 20;
    private int? _nextPageIndex;

    public ReadOnlyObservableCollection<FriendRequest> FriendRequests => friendRequestState.Items;
    public bool HasMoreFriendRequests => _nextPageIndex.HasValue;

    [ObservableProperty] public partial bool IsLoadingFriendRequests { get; private set; }

    public override async Task OnNavigatedToAsync(
        object? parameter,
        CancellationToken cancellationToken)
    {
        await LoadFriendRequestsPageAsync(1, true, cancellationToken);
    }

    [RelayCommand]
    private async Task LoadMoreFriendRequests(CancellationToken cancellationToken)
    {
        if (IsLoadingFriendRequests || _nextPageIndex is not { } nextPageIndex)
        {
            return;
        }

        await LoadFriendRequestsPageAsync(nextPageIndex, false, cancellationToken);
    }

    private async Task LoadFriendRequestsPageAsync(
        int pageIndex,
        bool replace,
        CancellationToken cancellationToken = default)
    {
        IsLoadingFriendRequests = true;
        try
        {
            var pendingFriendRequests = await friendsApiClient.GetFriendRequestsAsync(
                pageIndex,
                FriendRequestPageSize,
                cancellationToken);

            if (pendingFriendRequests.IsFailure)
            {
                toastService.ShowError(pendingFriendRequests.Error);
            }
            else
            {
                var requests = pendingFriendRequests.Value.Items.Select(FriendRequestMapper.Map).ToList();
                if (replace)
                {
                    var liveRequests = FriendRequests
                        .Where(current => requests.All(request => request.Id != current.Id))
                        .ToList();
                    friendRequestState.Set(requests.Concat(liveRequests));
                }
                else
                {
                    foreach (var request in requests)
                    {
                        friendRequestState.Add(request);
                    }
                }

                _nextPageIndex = pendingFriendRequests.Value.Pagination.NextPageIndex;
                friendRequestState.SetPendingCount(
                    pendingFriendRequests.Value.Pagination.TotalCount);
                OnPropertyChanged(nameof(HasMoreFriendRequests));
            }
        }
        finally
        {
            IsLoadingFriendRequests = false;
        }
    }

    [RelayCommand]
    private async Task AcceptFriendRequest(FriendRequest friendRequest)
    {
        var acceptResult = await friendsApiClient.AcceptFriendRequestAsync(friendRequest.Id);
        if (acceptResult.IsFailure)
        {
            toastService.ShowError(acceptResult.Error);
        }
        else
        {
            friendRequestState.Remove(friendRequest.Id);
            friendRequestState.DecrementPendingCount();
            friendState.Add(FriendMapper.Map(acceptResult.Value));
            directChatState.Add(DirectChatMapper.Map(acceptResult.Value));
            toastService.ShowSuccess($"Friend request from '{friendRequest.SenderUsername}' accepted.");
        }
    }

    [RelayCommand]
    private async Task DeclineFriendRequest(FriendRequest friendRequest)
    {
        var declineResult = await friendsApiClient.RejectFriendRequestAsync(friendRequest.Id);
        if (declineResult.IsFailure)
        {
            toastService.ShowError(declineResult.Error);
        }
        else
        {
            friendRequestState.Remove(friendRequest.Id);
            friendRequestState.DecrementPendingCount();
            toastService.ShowSuccess($"Friend request from '{friendRequest.SenderUsername}' declined.");
        }
    }
}
