namespace Chatly.WebApi.Features.FriendRequests.Endpoints.SendFriendRequest;

internal sealed record SendFriendRequestCommand(UserId ReceiverId) : ICommand;
