namespace Chatly.WebApi.Features.Friendships.Endpoints.RemoveFriendship;

public sealed record RemoveFriendshipCommand(UserId AssociatedUserId) : ICommand;