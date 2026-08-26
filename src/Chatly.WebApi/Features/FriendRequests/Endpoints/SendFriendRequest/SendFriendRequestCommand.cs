using Chatly.WebApi.Features.Users.Models;
using Mediator;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.SendFriendRequest;

public sealed record SendFriendRequestCommand(UserId ReceiverId) : ICommand;