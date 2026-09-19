using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;
using Chatly.Contracts.Features.FriendRequests.Models;

namespace Chatly.Desktop.Mappers;

internal static class FriendRequestMapper
{
    public static FriendRequest Map(FriendRequestContract response) =>
        new()
        {
            Id = response.FriendRequestId,
            SenderUsername = response.SenderUsername,
            ProfilePictureUrl = response.SenderProfilePictureUrl
        };

    public static FriendRequest Map(IncomingFriendRequestNotification message) => Map(message.Request);
}