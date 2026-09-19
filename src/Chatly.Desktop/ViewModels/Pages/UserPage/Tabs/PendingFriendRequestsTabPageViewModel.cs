using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Chatly.Contracts.Common.Pagination;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Services.Api.Clients;
using Chatly.Desktop.Services.Friendships;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;

[SingletonService]
public sealed partial class PendingFriendRequestsTabPageViewModel : PageViewModelBase
{
    private readonly FriendRequestState _friendRequestState;
    private readonly ObservableCollection<PendingFriendRequestViewModel> _friendRequests = [];
    private readonly FriendsApiClient _friendsApiClient;
    private readonly FriendshipStateService _friendshipStateService;
    private readonly IToastService _toastService;
    private int? _nextPageIndex;

    public PendingFriendRequestsTabPageViewModel(
        IToastService toastService,
        FriendsApiClient friendsApiClient,
        FriendRequestState friendRequestState,
        FriendshipStateService friendshipStateService)
    {
        _toastService = toastService;
        _friendsApiClient = friendsApiClient;
        _friendRequestState = friendRequestState;
        _friendshipStateService = friendshipStateService;
        FriendRequests = new ReadOnlyObservableCollection<PendingFriendRequestViewModel>(_friendRequests);

        foreach (var friendRequest in friendRequestState.Items)
        {
            _friendRequests.Add(CreateFriendRequestViewModel(friendRequest));
        }

        friendRequestState.CollectionChanged += FriendRequests_CollectionChanged;
    }

    public ReadOnlyObservableCollection<PendingFriendRequestViewModel> FriendRequests { get; }

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
            var pendingFriendRequests = await _friendsApiClient.GetFriendRequestsAsync(
                pageIndex,
                PaginationPolicy.DefaultPageSize,
                cancellationToken);

            if (pendingFriendRequests.IsFailure)
            {
                _toastService.ShowError(pendingFriendRequests.Error);
            }
            else
            {
                var requests = pendingFriendRequests.Value.Items.Select(FriendRequestMapper.Map).ToList();
                if (replace)
                {
                    var liveRequests = _friendRequestState.Items
                        .Where(current => requests.All(request => request.Id != current.Id))
                        .ToList();
                    _friendRequestState.Set(requests.Concat(liveRequests));
                }
                else
                {
                    foreach (var request in requests)
                    {
                        _friendRequestState.Add(request);
                    }
                }

                _nextPageIndex = pendingFriendRequests.Value.Pagination.NextPageIndex;
                _friendRequestState.SetPendingCount(
                    pendingFriendRequests.Value.Pagination.TotalCount);
                OnPropertyChanged(nameof(HasMoreFriendRequests));
            }
        }
        finally
        {
            IsLoadingFriendRequests = false;
        }
    }

    private void FriendRequests_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            _friendRequests.Clear();
            return;
        }

        if (e.OldItems is not null)
        {
            foreach (FriendRequest friendRequest in e.OldItems)
            {
                var existing = _friendRequests.FirstOrDefault(item => item.Id == friendRequest.Id);
                if (existing is not null)
                {
                    _friendRequests.Remove(existing);
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (FriendRequest friendRequest in e.NewItems)
            {
                if (_friendRequests.All(item => item.Id != friendRequest.Id))
                {
                    _friendRequests.Add(CreateFriendRequestViewModel(friendRequest));
                }
            }
        }
    }

    private PendingFriendRequestViewModel CreateFriendRequestViewModel(FriendRequest friendRequest) =>
        new(
            friendRequest,
            _friendsApiClient,
            _friendRequestState,
            _friendshipStateService,
            _toastService);
}