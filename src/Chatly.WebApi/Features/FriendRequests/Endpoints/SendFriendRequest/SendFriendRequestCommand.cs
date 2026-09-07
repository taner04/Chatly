namespace Chatly.WebApi.Features.FriendRequests.Endpoints.SendFriendRequest;

public sealed record SendFriendRequestCommand(UserId ReceiverId) : ICommand;