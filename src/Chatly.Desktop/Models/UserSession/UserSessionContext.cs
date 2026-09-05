namespace Chatly.Desktop.Models.UserSession;

[SingletonService]
public sealed partial class UserSessionContext : ObservableObject
{
    [ObservableProperty] public partial User? CurrentUser { get; private set; }

    internal string? AccessToken { get; private set; }

    internal void SetAccessToken(string accessToken)
    {
        AccessToken = accessToken;
    }

    internal void SetAuthenticated(User user)
    {
        CurrentUser = user;
    }

    internal void Clear()
    {
        AccessToken = null;
        CurrentUser = null;
        Friends.Clear();
        OnlineFriends.Clear();
        DirectChats.Clear();
        FriendRequests.Clear();
        SetPendingFriendRequestCount(0);
    }
}