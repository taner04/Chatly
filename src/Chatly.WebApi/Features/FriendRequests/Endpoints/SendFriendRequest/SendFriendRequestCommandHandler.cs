using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;
using Chatly.Contracts.Features.FriendRequests.Models;
using Chatly.WebApi.Features.FriendRequests.Enums;
using Chatly.WebApi.Features.FriendRequests.Exception;
using Chatly.WebApi.Features.FriendRequests.Models;
using Chatly.WebApi.Features.Users.Services.ProfilePictures;
using Chatly.WebApi.Features.Users.Services.Profiles;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.SendFriendRequest;

internal sealed class SendFriendRequestCommandHandler(
    ChatlyDbContext context,
    CurrentUserService currentUser,
    ProfilePictureUrlFactory profilePictureUrlFactory,
    NotificationPublisher notificationPublisher)
    : ICommandHandler<SendFriendRequestCommand>
{
    public async ValueTask<Unit> Handle(
        SendFriendRequestCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetCurrentUserId();
        if (command.ReceiverId == userId)
        {
            throw new FriendRequestToSelfException();
        }

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
            .SelectProfile()
            .SingleAsync(cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        await notificationPublisher.PublishAsync(command.ReceiverId, new IncomingFriendRequestNotification(
            new FriendRequestContract(
                friendRequest.Id.Value,
                userId.Value,
                sender.Username!,
                profilePictureUrlFactory.CreateProfilePictureUrl(sender.ProfilePictureKey))));

        return Unit.Value;
    }
}