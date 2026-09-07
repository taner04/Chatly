using Chatly.Contracts.Endpoints.FriendRequests.Results;

namespace Chatly.Desktop.Mappers;

public static class FriendRequestMapper
{
    public static FriendRequest Map(GetFriendRequestsResponse response)
    {
        return new FriendRequest
        {
            Id = response.FriendRequestId,
            SenderUsername = response.Username,
            ProfilePictureUrl = response.ProfilePictureUrl
        };
    }

    public static FriendRequest Map(IncomingFriendRequestMessage message)
    {
        return new FriendRequest
        {
            Id = message.FriendRequestId,
            SenderUsername = message.SenderUsername,
            ProfilePictureUrl = message.SenderProfilePictureUrl?.ToString()
        };
    }
}