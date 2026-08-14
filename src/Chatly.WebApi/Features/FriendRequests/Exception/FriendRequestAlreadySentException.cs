using System.Net;
using Chatly.WebApi.Common.Shared.Exceptions;
using Chatly.WebApi.Features.Users.Models;

namespace Chatly.WebApi.Features.FriendRequests.Exception;

public sealed class FriendRequestAlreadySentException(UserId userId)
    : ChatlyException(
        "Friend request already sent",
        $"A friend request to user ID '{userId.Value}' has already been sent.",
        "FriendRequest.AlreadySent",
        HttpStatusCode.Conflict);
