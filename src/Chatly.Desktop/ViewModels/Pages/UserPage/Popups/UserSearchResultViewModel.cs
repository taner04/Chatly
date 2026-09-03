using Chatly.Contracts.Endpoints.Users.Results;

namespace Chatly.Desktop.ViewModels.Pages.UserPage.Popups;

public sealed partial class UserSearchResultViewModel(UserSearchResponse response) : ObservableObject
{
    public Guid UserId { get; } = response.UserId;

    public string Username { get; } = response.Username;

    public Uri? ProfilePictureUrl { get; } = response.ProfilePictureUrl;

    [ObservableProperty]
    public partial UserRelationshipStatus RelationshipStatus { get; set; } = response.RelationshipStatus;

    public bool CanSendFriendRequest => RelationshipStatus == UserRelationshipStatus.None;

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
}