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

    internal void UpdateUserProfile(Guid userId, string? username, string? profilePictureUrl)
    {
        if (CurrentUser?.Id != userId)
        {
            return;
        }

        CurrentUser.Username = username;
        CurrentUser.ProfilePictureUrl = profilePictureUrl;
    }

    internal void Clear()
    {
        AccessToken = null;
        CurrentUser = null;
    }
}
