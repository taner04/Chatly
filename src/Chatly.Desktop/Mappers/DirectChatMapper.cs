using Chatly.Contracts.Features.Chats.Endpoints.GetChats;
using Chatly.Contracts.Features.Friendships.Models;

namespace Chatly.Desktop.Mappers;

internal static class DirectChatMapper
{
    public static DirectChat Map(GetChatsResponse response, UserRegistry userRegistry) =>
        new()
        {
            Id = response.ChatId,
            UnreadMessageCount = response.UnreadMessageCount,
            User = userRegistry.GetOrAdd(
                response.AssociatedUserId,
                response.AssociatedUsername,
                response.AssociatedProfilePictureUrl,
                response.IsOnline)
        };

    public static DirectChat Map(FriendshipContract friendship, UserRegistry userRegistry) =>
        new()
        {
            Id = friendship.DirectChatId,
            UnreadMessageCount = 0,
            User = userRegistry.GetOrAdd(
                friendship.FriendUserId,
                friendship.FriendUsername,
                friendship.FriendProfilePictureUrl,
                friendship.IsOnline)
        };
}