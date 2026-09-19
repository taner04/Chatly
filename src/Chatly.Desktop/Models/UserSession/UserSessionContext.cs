namespace Chatly.Desktop.Models.UserSession;

[SingletonService]
public sealed partial class UserSessionContext(UserRegistry userRegistry) : ObservableObject
{
    [ObservableProperty] public partial User? CurrentUser { get; private set; }

    internal string? AccessToken { get; private set; }

    internal void SetAccessToken(string accessToken)
    {
        AccessToken = accessToken;
    }

    internal void SetAuthenticated(User user)
    {
        CurrentUser = userRegistry.GetOrAdd(user);
    }

    internal void Clear()
    {
        AccessToken = null;
        CurrentUser = null;
        userRegistry.Clear();
    }
}