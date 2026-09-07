namespace Chatly.WebApi.Features.FriendRequests.Endpoints.RejectFriendRequest;

public sealed record RejectFriendRequestCommand(Guid FriendRequestId) : ICommand;