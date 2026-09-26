using System.Net;

namespace Chatly.WebApi.Features.FriendRequests.Exception;

internal sealed class FriendRequestToSelfException()
    : ChatlyException(
        "Friend request to yourself",
        "You cannot send a friend request to yourself.",
        "FriendRequest.ToSelf",
        HttpStatusCode.BadRequest);
