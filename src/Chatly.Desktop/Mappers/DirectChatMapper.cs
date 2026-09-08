using Chatly.Contracts.Endpoints.Chats.Results;
using Chatly.Contracts.Endpoints.FriendRequests.Results;

namespace Chatly.Desktop.Mappers;

internal static class DirectChatMapper
{
    public static DirectChat Map(GetChatsResponse response)
    {
        return new DirectChat
        {
            Id = response.ChatId,
            UnreadMessageCount = response.UnreadMessageCount,
            User = new User
            {
                Id = response.AssociatedUserId,
                Username = response.AssociatedUsername,
                ProfilePictureUrl = response.AssociatedProfilePictureUrl,
                IsOnline = response.IsOnline
            }
        };
    }

    public static DirectChat Map(AcceptFriendRequestResponse response)
    {
        return new DirectChat
        {
            Id = response.DirectChatId,
            UnreadMessageCount = 0,
            User = new User
            {
                Id = response.FriendUserId,
                Username = response.FriendUsername,
                ProfilePictureUrl = response.FriendProfilePictureUrl,
                IsOnline = response.IsOnline
            }
        };
    }

    public static DirectChat Map(FriendRequestAcceptedMessage message)
    {
        return new DirectChat
        {
            Id = message.DirectChatId,
            UnreadMessageCount = 0,
            User = new User
            {
                Id = message.FriendUserId,
                Username = message.FriendUsername,
                ProfilePictureUrl = message.FriendProfilePictureUrl,
                IsOnline = message.IsOnline
            }
        };
    }
}
