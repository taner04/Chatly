using Chatly.WebApi.Features.FriendRequests.Enums;
using Chatly.WebApi.Features.FriendRequests.Models;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.RejectFriendRequest;

public sealed class RejectFriendRequestCommandHandler(
    CurrentUserService currentUserService,
    ChatlyDbContext context) : ICommandHandler<RejectFriendRequestCommand>
{
    public async ValueTask<Unit> Handle(RejectFriendRequestCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        var friendRequestId = FriendRequestId.From(command.FriendRequestId);

        var friendRequest = await context.FriendRequests
            .FirstOrDefaultAsync(fr =>
                    fr.Id == friendRequestId &&
                    fr.ReceiverUserId == userId &&
                    fr.Status == FriendRequestStatus.Pending,
                cancellationToken) ?? throw new EntityNotFoundException<FriendRequest>(command.FriendRequestId);

        friendRequest.Status = FriendRequestStatus.Rejected;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}