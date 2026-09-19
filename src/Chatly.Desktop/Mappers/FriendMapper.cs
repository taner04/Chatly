using Chatly.Contracts.Features.Friendships.Models;

namespace Chatly.Desktop.Mappers;

internal static class FriendMapper
{
    public static Friend Map(FriendshipContract friendship, UserRegistry userRegistry) =>
        new()
        {
            ChatId = friendship.DirectChatId,
            User = userRegistry.GetOrAdd(
                friendship.FriendUserId,
                friendship.FriendUsername,
                friendship.FriendProfilePictureUrl,
                friendship.IsOnline)
        };
}