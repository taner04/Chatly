using Chatly.Contracts.Endpoints.FriendRequests.Results;
using Chatly.WebApi.Features.Chats.Models;
using Chatly.WebApi.Features.FriendRequests.Enums;
using Chatly.WebApi.Features.FriendRequests.Models;
using Chatly.WebApi.Features.Friendships.Models;
using Chatly.WebApi.Features.Hubs;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.AcceptFriendRequest;

public sealed class AcceptFriendRequestCommandHandler(
    CurrentUserService currentUserService,
    ChatlyDbContext context,
    AzureBlobService blobService,
    NotificationPublisher notificationPublisher,
    OnlinePresenceTracker presenceTracker)
    : ICommandHandler<AcceptFriendRequestCommand, AcceptFriendRequestResponse>
{
    public async ValueTask<AcceptFriendRequestResponse> Handle(
        AcceptFriendRequestCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        var friendRequestId = FriendRequestId.From(command.FriendRequestId);

        var friendRequest = await context.FriendRequests
            .FirstOrDefaultAsync(fr =>
                    fr.Id == friendRequestId &&
                    fr.ReceiverUserId == userId &&
                    fr.Status == FriendRequestStatus.Pending,
                cancellationToken) ?? throw new EntityNotFoundException<FriendRequest>(command.FriendRequestId);

        friendRequest.Status = FriendRequestStatus.Accepted;

        var friendship = new Friendship(friendRequest.SenderUserId, friendRequest.ReceiverUserId);
        context.Friendships.Add(friendship);

        var chat = await context.Chats.SingleOrDefaultAsync(
            candidate => candidate.FirstUserId == friendship.FirstUserId &&
                         candidate.SecondUserId == friendship.SecondUserId,
            cancellationToken);

        if (chat is null)
        {
            chat = new Chat(friendship.FirstUserId, friendship.SecondUserId);
            context.Chats.Add(chat);
        }

        var friend = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == friendRequest.SenderUserId)
            .Select(user => new
            {
                user.Id,
                user.Username,
                user.ProfilePictureKey
            })
            .SingleAsync(cancellationToken);

        var currentUser = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new
            {
                user.Id,
                user.Username,
                user.ProfilePictureKey
            })
            .SingleAsync(cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        await notificationPublisher.PublishAsync(
            friendRequest.SenderUserId,
            new FriendRequestAcceptedMessage(
                friendship.Id.Value,
                chat.Id.Value,
                currentUser.Id.Value,
                currentUser.Username!,
                blobService.CreateReadUrl(currentUser.ProfilePictureKey)?.ToString(),
                presenceTracker.IsOnline(currentUser.Id)));

        return new AcceptFriendRequestResponse(
            friendship.Id.Value,
            chat.Id.Value,
            friend.Id.Value,
            friend.Username!,
            blobService.CreateReadUrl(friend.ProfilePictureKey)?.ToString(),
            presenceTracker.IsOnline(friend.Id));
    }
}