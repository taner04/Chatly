namespace Chatly.WebApi.Features.FriendRequests.Endpoints.RejectFriendRequest;

internal sealed record RejectFriendRequestCommand(Guid FriendRequestId) : ICommand;
