using System.Net;
using Chatly.WebApi.Common.Shared.Exceptions;
using Chatly.WebApi.Features.FriendRequests.Models;

namespace Chatly.WebApi.Features.FriendRequests.Exception;

internal sealed class IncomingFriendRequestAlreadyExistsException(
    FriendRequestId requestId)
    : ChatlyException(
        "Incoming friend request already exists",
        $"An incoming friend request with ID '{requestId.Value}' already exists.",
        "FriendRequest.IncomingAlreadyExists",
        HttpStatusCode.Conflict);