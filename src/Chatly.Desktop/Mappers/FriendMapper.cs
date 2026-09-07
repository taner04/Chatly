using Chatly.Contracts.Endpoints.FriendRequests.Results;
using Chatly.Contracts.Endpoints.Friendships.Results;

namespace Chatly.Desktop.Mappers;

public static class FriendMapper
{
    public static Friend Map(GetFriendshipsResponse response)
    {
        return new Friend
        {
            ChatId = response.DirectChatId,
            User = new User
            {
                Id = response.FriendUserId,
                Username = response.FriendUsername,
                ProfilePictureUrl = response.FriendProfilePictureUrl,
                IsOnline = response.IsOnline
            }
        };
    }

    public static Friend Map(AcceptFriendRequestResponse response)
    {
        return new Friend
        {
            ChatId = response.DirectChatId,
            User = new User
            {
                Id = response.FriendUserId,
                Username = response.FriendUsername,
                ProfilePictureUrl = response.FriendProfilePictureUrl,
                IsOnline = response.IsOnline
            }
        };
    }

    public static Friend Map(FriendRequestAcceptedMessage message)
    {
        return new Friend
        {
            ChatId = message.DirectChatId,
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