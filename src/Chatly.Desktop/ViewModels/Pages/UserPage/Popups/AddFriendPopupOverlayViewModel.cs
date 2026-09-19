using System.Collections.ObjectModel;
using Chatly.Contracts.Features.Users.Endpoints.SearchUsers;
using Chatly.Desktop.Services.Api.Clients;
using Chatly.Desktop.ViewModels.Popups;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Popups;

[TransientService]
public sealed partial class AddFriendPopupOverlayViewModel(
    IToastService toastService,
    FriendsApiClient friendsApiClient,
    UserApiClient userApiClient) : PopupOverlayViewModel
{
    private string _activeSearch = string.Empty;
    private int? _nextPageIndex;

    public override string Title => "Add Friend";

    [ObservableProperty] public partial string UserName { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsSearching { get; private set; }
    [ObservableProperty] public partial bool HasSearched { get; private set; }

    public ObservableCollection<UserSearchResultViewModel> SearchedUsers { get; } = [];
    public bool HasMoreUsers => _nextPageIndex.HasValue;
    public bool HasNoSearchResults => HasSearched && !IsSearching && SearchedUsers.Count == 0;

    public override void CloseOverlay()
    {
        SearchForUsersCommand.Cancel();
        LoadMoreUsersCommand.Cancel();
        ClearSearchResults();
        base.CloseOverlay();
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
        ClearSearchResults();
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
                    SearchedUsers.Add(new UserSearchResultViewModel(
                        user,
                        friendsApiClient,
                        toastService));
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

    private void ClearSearchResults()
    {
        foreach (var user in SearchedUsers)
        {
            user.CancelPendingRequest();
        }

        SearchedUsers.Clear();
    }
}