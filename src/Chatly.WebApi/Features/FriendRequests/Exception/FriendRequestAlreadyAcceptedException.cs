using System.Net;

namespace Chatly.WebApi.Features.FriendRequests.Exception;

internal sealed class FriendRequestAlreadyAcceptedException(UserId userId)
    : ChatlyException(
        "Friend request already accepted",
        $"The friend request involving user ID '{userId.Value}' has already been accepted.",
        "FriendRequest.AlreadyAccepted",
        HttpStatusCode.Conflict);