using Chatly.Contracts.Users.Results;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Chatly.Desktop.Features.Users;

public sealed partial class UserSessionContext : ObservableObject
{
    [ObservableProperty] public partial CurrentUserResponse? CurrentUser { get; private set; }

    [ObservableProperty] public partial bool IsAuthenticated { get; private set; }

    internal string? AccessToken { get; private set; }

    internal void SetAccessToken(string accessToken)
    {
        AccessToken = accessToken;
    }

    internal void SetAuthenticated(CurrentUserResponse user)
    {
        CurrentUser = user;
        IsAuthenticated = true;
    }

    internal void Clear()
    {
        AccessToken = null;
        CurrentUser = null;
        IsAuthenticated = false;
    }
}