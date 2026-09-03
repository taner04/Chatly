using System.Linq;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Services.Api;

namespace Chatly.Desktop.Services.Authentication;

[SingletonService]
public sealed class SessionService(
    AuthenticationService authenticationService,
    UserWebService userWebService,
    FriendsWebService friendsWebService,
    ChatWebService chatWebService,
    UserSessionContext sessionContext)
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var accessToken = await authenticationService.AuthenticateAsync(cancellationToken);

        sessionContext.SetAccessToken(accessToken);
        try
        {
            var userResult = await userWebService.GetCurrentUserAsync(cancellationToken);
            if (userResult.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve the current user: {userResult.Error.Detail}");
            }

            sessionContext.SetAuthenticated(UserMapper.Map(userResult.Value));

            var friendshipsResult = await friendsWebService.GetFriendshipsAsync(cancellationToken);
            if (friendshipsResult.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve friendships: {friendshipsResult.Error.Detail}");
            }

            sessionContext.SetFriends(friendshipsResult.Value.Select(FriendMapper.Map));

            var chatsResult = await chatWebService.GetChatsAsync(cancellationToken);
            if (chatsResult.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve chats: {chatsResult.Error.Detail}");
            }

            sessionContext.SetDirectChats(chatsResult.Value.Select(DirectChatMapper.Map));
        }
        catch
        {
            sessionContext.Clear();
            throw;
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        try
        {
            await authenticationService.LogoutAsync(cancellationToken);
        }
        finally
        {
            sessionContext.Clear();
        }
    }
}