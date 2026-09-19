using Chatly.WebApi.Features.FriendRequests.Enums;
using Chatly.WebApi.Features.FriendRequests.Services;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.RejectFriendRequest;

internal sealed class RejectFriendRequestCommandHandler(
    ChatlyDbContext context,
    PendingIncomingFriendRequestService pendingRequestService) : ICommandHandler<RejectFriendRequestCommand>
{
    public async ValueTask<Unit> Handle(RejectFriendRequestCommand command, CancellationToken cancellationToken)
    {
        var friendRequest = await pendingRequestService.GetAsync(
            command.FriendRequestId,
            cancellationToken);

        friendRequest.Status = FriendRequestStatus.Rejected;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}