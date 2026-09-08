using Chatly.Contracts.SignalR;
using Chatly.WebApi.Features.FriendRequests.Enums;
using Chatly.WebApi.Features.Friendships.Models;

namespace Chatly.WebApi.Features.Friendships.Endpoints.RemoveFriendship;

internal sealed class RemoveFriendshipCommandHandler(
    CurrentUserService userService,
    ChatlyDbContext context,
    NotificationPublisher notificationPublisher) : ICommandHandler<RemoveFriendshipCommand>
{
    public async ValueTask<Unit> Handle(RemoveFriendshipCommand command, CancellationToken cancellationToken)
    {
        var userId = userService.GetCurrentUserId();

        var userPair = UserPair.Create(userId, command.AssociatedUserId);

        var friendship = await context.Friendships
                             .FirstOrDefaultAsync(
                                 f => f.FirstUserId == userPair.FirstUserId && f.SecondUserId == userPair.SecondUserId,
                                 cancellationToken)
                         ?? throw new EntityNotFoundException<Friendship>(command.AssociatedUserId.Value);

        context.Friendships.Remove(friendship);

        var acceptedRequest = await context.FriendRequests.FirstOrDefaultAsync(
            request => request.FirstUserId == userPair.FirstUserId &&
                       request.SecondUserId == userPair.SecondUserId &&
                       request.Status == FriendRequestStatus.Accepted,
            cancellationToken);

        if (acceptedRequest is not null)
        {
            context.FriendRequests.Remove(acceptedRequest);
        }

        await context.SaveChangesAsync(cancellationToken);

        await notificationPublisher.PublishAsync(
            command.AssociatedUserId,
            new FriendshipRemovedMessage(userId.Value));

        return Unit.Value;
    }
}
