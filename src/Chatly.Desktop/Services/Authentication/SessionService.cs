using Chatly.Desktop.Mappers;
using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.Services.Api.Clients;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Authentication;

[SingletonService]
public sealed class SessionService(
    AuthenticationService authenticationService,
    AppSettings appSettings,
    UserApiClient userApiClient,
    DeviceSessionApiClient deviceSessionApiClient,
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
        await authenticationService.AuthenticateAsync(cancellationToken);

        if (appSettings.DeviceSettings.DeviceId is null)
        {
            appSettings.DeviceSettings.DeviceId = Guid.CreateVersion7();
            appSettings.Save();
        }

        try
        {
            var userResult = await userApiClient.GetCurrentUserAsync(cancellationToken);
            if (userResult is { IsFailure: true, Error.ErrorCode: DeviceSessionErrorCodes.Revoked })
            {
                await LogoutAsync(cancellationToken);
                await StartAsync(cancellationToken);
                return;
            }

            if (userResult.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve the current user: {userResult.Error.Detail}");
            }

            sessionContext.SetAuthenticated(UserMapper.Map(userResult.Value));
            await RefreshAsync(cancellationToken);
        }
        catch
        {
            ClearState();
            throw;
        }
    }

    internal async Task RefreshAsync(CancellationToken cancellationToken)
    {
        var friendshipsResult = await friendsApiClient.GetFriendshipsAsync(cancellationToken);
        if (friendshipsResult.IsFailure)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve friendships: {friendshipsResult.Error.Detail}");
        }

        var pendingFriendRequestsResult = await friendsApiClient.GetFriendRequestsAsync(1, 1, cancellationToken);
        if (pendingFriendRequestsResult.IsFailure)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve pending friend request count: {pendingFriendRequestsResult.Error.Detail}");
        }

        var chatsResult = await chatApiClient.GetChatsAsync(cancellationToken);
        if (chatsResult.IsFailure)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve chats: {chatsResult.Error.Detail}");
        }

        UiThreadDispatcher.SafeInvoke(() =>
        {
            friendState.Set(friendshipsResult.Value.Select(friendship => FriendMapper.Map(friendship, userRegistry)));
            friendRequestState.SetPendingCount(pendingFriendRequestsResult.Value.Pagination.TotalCount);
            directChatState.Set(chatsResult.Value.Select(chat => DirectChatMapper.Map(chat, userRegistry)));
        });
    }

    internal async Task LogoutAsync(CancellationToken cancellationToken)
    {
        try
        {
            await deviceSessionApiClient.RevokeCurrentDeviceSessionAsync(cancellationToken);
            await authenticationService.LogoutAsync(cancellationToken);
        }
        finally
        {
            ResetDeviceId();
            ClearState();
        }
    }

    private void ResetDeviceId()
    {
        appSettings.DeviceSettings.DeviceId = null;
        appSettings.Save();
    }

    private void ClearState()
    {
        sessionContext.Clear();
        friendState.Clear();
        directChatState.Clear();
        friendRequestState.Clear();
    }
}