using Chatly.Contracts.Endpoints.FriendRequests.Results;
using Chatly.WebApi.Features.FriendRequests.Enums;
using Chatly.WebApi.Features.FriendRequests.Exception;
using Chatly.WebApi.Features.FriendRequests.Models;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.SendFriendRequest;

public sealed class SendFriendRequestCommandHandler(
    ChatlyDbContext context,
    CurrentUserService currentUser,
    AzureBlobService blobService,
    NotificationPublisher notificationPublisher)
    : ICommandHandler<SendFriendRequestCommand>
{
    public async ValueTask<Unit> Handle(
        SendFriendRequestCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetCurrentUserId();

        var receiverExists = await context.Users
            .AnyAsync(user => user.Id == command.ReceiverId, cancellationToken);

        if (!receiverExists)
        {
            throw new EntityNotFoundException<User>(command.ReceiverId.Value);
        }

        var userPair = UserPair.Create(userId, command.ReceiverId);

        var existingRequest = await context.FriendRequests.SingleOrDefaultAsync(
            request =>
                request.FirstUserId == userPair.FirstUserId &&
                request.SecondUserId == userPair.SecondUserId,
            cancellationToken);

        FriendRequest friendRequest;

        if (existingRequest is null)
        {
            friendRequest = new FriendRequest(userId, command.ReceiverId);
            context.FriendRequests.Add(friendRequest);
        }
        else
        {
            switch (existingRequest.Status)
            {
                case FriendRequestStatus.Pending
                    when existingRequest.SenderUserId == userId:
                    throw new FriendRequestAlreadySentException(command.ReceiverId);

                case FriendRequestStatus.Pending:
                    throw new IncomingFriendRequestAlreadyExistsException(
                        existingRequest.Id);

                case FriendRequestStatus.Accepted:
                    throw new FriendRequestAlreadyAcceptedException(
                        command.ReceiverId);

                case FriendRequestStatus.Rejected:
                    existingRequest.SenderUserId = userId;
                    existingRequest.ReceiverUserId = command.ReceiverId;
                    existingRequest.Status = FriendRequestStatus.Pending;
                    friendRequest = existingRequest;
                    break;

                default:
                    throw new NotImplementedException(
                        $"Friend request status '{existingRequest.Status}' is not handled.");
            }
        }

        var sender = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new
            {
                user.Username,
                user.ProfilePictureKey
            })
            .SingleAsync(cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        await notificationPublisher.PublishAsync(command.ReceiverId, new IncomingFriendRequestMessage(
            friendRequest.Id.Value,
            userId.Value,
            sender.Username!,
            blobService.CreateReadUrl(sender.ProfilePictureKey)));

        return Unit.Value;
    }
}