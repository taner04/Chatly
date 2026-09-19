using Chatly.Contracts.Features.Friendships.Models;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.AcceptFriendRequest;

internal sealed record AcceptFriendRequestCommand(Guid FriendRequestId)
    : ICommand<FriendshipContract>;