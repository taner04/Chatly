using System.Collections.ObjectModel;
using System.Linq;
using Chatly.Contracts.Endpoints.FriendRequests.Requests;
using Chatly.Contracts.Endpoints.Users.Requests;
using Chatly.Contracts.Endpoints.Users.Results;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Extensions;
using Chatly.Desktop.Services.Api;
using Chatly.Desktop.ViewModels.Popups;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Popups;

[TransientService]
public sealed partial class AddFriendPopupOverlayViewModel(
    IToastService toastService,
    FriendsApiClient friendsApiClient,
    UserApiClient userApiClient) : PopupOverlayViewModel
{
    private const int SearchPageSize = 20;
    private string _activeSearch = string.Empty;
    private int? _nextPageIndex;

    public override string Title => "Add Friend";

    [ObservableProperty] public partial string UserName { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsSearching { get; private set; }
    [ObservableProperty] public partial bool HasSearched { get; private set; }

    public ObservableCollection<UserSearchResultViewModel> SearchedUsers { get; } = [];
    public bool HasMoreUsers => _nextPageIndex.HasValue;
    public bool HasNoSearchResults => HasSearched && !IsSearching && SearchedUsers.Count == 0;

    [RelayCommand]
    private async Task SendFriendRequest(UserSearchResultViewModel user)
    {
        var sendFriendRequestResult =
            await friendsApiClient.SendFriendRequestAsync(new SendFriendRequestRequest(user.UserId));
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
    private async Task SearchForUsers(CancellationToken cancellationToken)
    {
        var search = UserName.Trim();
        if (string.IsNullOrWhiteSpace(search) || IsSearching)
        {
            return;
        }

        _activeSearch = search;
        _nextPageIndex = null;
        HasSearched = false;
        SearchedUsers.Clear();
        NotifySearchResultsChanged();

        await LoadUsersPageAsync(search, 1, cancellationToken);
    }

    [RelayCommand]
    private async Task LoadMoreUsers(CancellationToken cancellationToken)
    {
        if (IsSearching || _nextPageIndex is not { } nextPageIndex)
        {
            return;
        }

        await LoadUsersPageAsync(_activeSearch, nextPageIndex, cancellationToken);
    }

    private async Task LoadUsersPageAsync(string search, int pageIndex, CancellationToken cancellationToken)
    {
        IsSearching = true;
        NotifySearchResultsChanged();

        var searchResult = await userApiClient.SearchUsersAsync(
            new SearchUsersRequest(search, pageIndex),
            cancellationToken);

        if (!string.Equals(_activeSearch, search, StringComparison.Ordinal) ||
            !string.Equals(UserName.Trim(), search, StringComparison.Ordinal))
        {
            IsSearching = false;
            NotifySearchResultsChanged();
            return;
        }

        if (searchResult.IsFailure)
        {
            toastService.ShowError(searchResult.Error);
            _nextPageIndex = null;
        }
        else
        {
            var existingUserIds = SearchedUsers.Select(user => user.UserId).ToHashSet();
            foreach (var user in searchResult.Value.Items
                         .Where(user => user.RelationshipStatus != UserRelationshipStatus.Friends))
            {
                if (existingUserIds.Add(user.UserId))
                {
                    SearchedUsers.Add(new UserSearchResultViewModel(user));
                }
            }

            _nextPageIndex = searchResult.Value.Pagination.NextPageIndex;
        }

        HasSearched = true;
        IsSearching = false;
        NotifySearchResultsChanged();
    }

    private void NotifySearchResultsChanged()
    {
        OnPropertyChanged(nameof(HasMoreUsers));
        OnPropertyChanged(nameof(HasNoSearchResults));
    }
}