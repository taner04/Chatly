using Chatly.Contracts.Features.FriendRequests.Endpoints.AcceptFriendRequest;
using Chatly.Contracts.Features.Friendships.Models;
using Chatly.WebApi.Features.FriendRequests.Enums;
using Chatly.WebApi.Features.FriendRequests.Services;
using Chatly.WebApi.Features.Friendships.Models;
using Chatly.WebApi.Features.Users.Services.ProfilePictures;
using Chatly.WebApi.Features.Users.Services.Profiles;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.AcceptFriendRequest;

internal sealed class AcceptFriendRequestCommandHandler(
    ChatlyDbContext context,
    PendingIncomingFriendRequestService pendingRequestService,
    ProfilePictureUrlFactory profilePictureUrlFactory,
    NotificationPublisher notificationPublisher,
    OnlinePresenceTracker presenceTracker)
    : ICommandHandler<AcceptFriendRequestCommand, FriendshipContract>
{
    public async ValueTask<FriendshipContract> Handle(
        AcceptFriendRequestCommand command,
        CancellationToken cancellationToken)
    {
        var friendRequest = await pendingRequestService.GetAsync(
            command.FriendRequestId,
            cancellationToken);
        var userId = friendRequest.ReceiverUserId;

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

        var userProfiles = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == friendRequest.SenderUserId || user.Id == userId)
            .SelectProfile()
            .ToListAsync(cancellationToken);

        var sender = userProfiles.Single(user => user.UserId == friendRequest.SenderUserId);
        var receiver = userProfiles.Single(user => user.UserId == userId);

        await context.SaveChangesAsync(cancellationToken);

        await notificationPublisher.PublishAsync(
            friendRequest.SenderUserId,
            new FriendRequestAcceptedNotification(CreateContract(friendship, chat, receiver)));

        return CreateContract(friendship, chat, sender);
    }

    private FriendshipContract CreateContract(Friendship friendship, Chat chat, UserProfileRow friend) =>
        new(
            friendship.Id.Value,
            chat.Id.Value,
            friend.UserId.Value,
            friend.Username!,
            profilePictureUrlFactory.CreateProfilePictureUrl(friend.ProfilePictureKey),
            presenceTracker.IsOnline(friend.UserId));
}