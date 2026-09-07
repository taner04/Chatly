using Chatly.Contracts.Endpoints.FriendRequests.Results;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.AcceptFriendRequest;

public sealed record AcceptFriendRequestCommand(Guid FriendRequestId)
    : ICommand<AcceptFriendRequestResponse>;