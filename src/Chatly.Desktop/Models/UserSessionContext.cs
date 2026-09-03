namespace Chatly.Desktop.Models;

[SingletonService]
public sealed partial class UserSessionContext : ObservableObject
{
    [ObservableProperty] public partial User? CurrentUser { get; private set; }

    [ObservableProperty] public partial bool IsAuthenticated { get; private set; }

    internal string? AccessToken { get; private set; }

    internal void SetAccessToken(string accessToken)
    {
        AccessToken = accessToken;
    }

    internal void SetAuthenticated(User user)
    {
        CurrentUser = user;
        IsAuthenticated = true;
    }

    internal void Clear()
    {
        AccessToken = null;
        CurrentUser = null;
        Friends.Clear();
        OnlineFriends.Clear();
        DirectChats.Clear();
        FriendRequests.Clear();
        IsAuthenticated = false;
    }
}