using System.Net;

namespace Chatly.WebApi.Features.FriendRequests.Exception;

public sealed class FriendRequestAlreadySentException(UserId userId)
    : ChatlyException(
        "Friend request already sent",
        $"A friend request to user ID '{userId.Value}' has already been sent.",
        "FriendRequest.AlreadySent",
        HttpStatusCode.Conflict);