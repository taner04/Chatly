using Chatly.Desktop.Mappers;
using Chatly.Desktop.Services.Api.Clients;

namespace Chatly.Desktop.Services.Authentication;

[SingletonService]
public sealed class SessionService(
    AuthenticationService authenticationService,
    UserApiClient userApiClient,
    FriendsApiClient friendsApiClient,
    ChatApiClient chatApiClient,
    UserSessionContext sessionContext,
    UserRegistry userRegistry,
    FriendState friendState,
    DirectChatState directChatState,
    FriendRequestState friendRequestState)
{
    internal async Task StartAsync(CancellationToken cancellationToken)
    {
        var accessToken = await authenticationService.AuthenticateAsync(cancellationToken);

        sessionContext.SetAccessToken(accessToken);
        try
        {
            var userResult = await userApiClient.GetCurrentUserAsync(cancellationToken);
            if (userResult.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve the current user: {userResult.Error.Detail}");
            }

            sessionContext.SetAuthenticated(UserMapper.Map(userResult.Value));

            var friendshipsResult = await friendsApiClient.GetFriendshipsAsync(cancellationToken);
            if (friendshipsResult.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve friendships: {friendshipsResult.Error.Detail}");
            }

            friendState.Set(friendshipsResult.Value.Select(friendship => FriendMapper.Map(friendship, userRegistry)));

            var pendingFriendRequestsResult = await friendsApiClient.GetFriendRequestsAsync(1, 1, cancellationToken);
            if (pendingFriendRequestsResult.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve pending friend request count: {pendingFriendRequestsResult.Error.Detail}");
            }

            friendRequestState.SetPendingCount(
                pendingFriendRequestsResult.Value.Pagination.TotalCount);

            var chatsResult = await chatApiClient.GetChatsAsync(cancellationToken);
            if (chatsResult.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve chats: {chatsResult.Error.Detail}");
            }

            directChatState.Set(chatsResult.Value.Select(chat => DirectChatMapper.Map(chat, userRegistry)));
        }
        catch
        {
            ClearState();
            throw;
        }
    }

    internal async Task LogoutAsync(CancellationToken cancellationToken)
    {
        try
        {
            await authenticationService.LogoutAsync(cancellationToken);
        }
        finally
        {
            ClearState();
        }
    }

    private void ClearState()
    {
        sessionContext.Clear();
        friendState.Clear();
        directChatState.Clear();
        friendRequestState.Clear();
    }
}