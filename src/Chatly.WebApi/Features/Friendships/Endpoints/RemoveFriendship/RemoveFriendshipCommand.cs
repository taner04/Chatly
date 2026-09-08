namespace Chatly.WebApi.Features.Friendships.Endpoints.RemoveFriendship;

internal sealed record RemoveFriendshipCommand(UserId AssociatedUserId) : ICommand;
