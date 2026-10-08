using Chatly.Contracts.Features.Friendships.Models;
using Chatly.WebApi.Features.Users.Services.ProfilePictures;

namespace Chatly.WebApi.Features.Friendships.Endpoints.GetFriendships;

internal sealed class GetFriendshipsQueryHandler(
    CurrentUserService currentUserService,
    ChatlyDbContext context,
    ProfilePictureUrlFactory profilePictureUrlFactory,
    OnlinePresenceTracker presenceTracker) : IQueryHandler<GetFriendshipsQuery, IReadOnlyList<FriendshipContract>>
{
    public async ValueTask<IReadOnlyList<FriendshipContract>> Handle(
        GetFriendshipsQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        var friendships = await (
                from friendship in context.Friendships.AsNoTracking().ForUser(userId)
                let friendUserId = friendship.FirstUserId == userId ? friendship.SecondUserId : friendship.FirstUserId
                join chat in context.Chats.AsNoTracking()
                    on new { friendship.FirstUserId, friendship.SecondUserId }
                    equals new { chat.FirstUserId, chat.SecondUserId }
                join friend in context.Users.AsNoTracking() on friendUserId equals friend.Id
                where friend.Username != null
                orderby friend.Username, friend.Id
                select new
                {
                    FriendshipId = friendship.Id.Value,
                    DirectChatId = chat.Id.Value,
                    FriendUserId = friend.Id.Value,
                    friend.Username,
                    ProfilePictureKey = friend.ProfilePictureFile == null
                        ? null
                        : friend.ProfilePictureFile.BlobName
                })
            .ToListAsync(cancellationToken);

        return
        [
            .. friendships.Select(friendship => new FriendshipContract(
                friendship.FriendshipId,
                friendship.DirectChatId,
                friendship.FriendUserId,
                friendship.Username!,
                profilePictureUrlFactory.CreateProfilePictureUrl(friendship.ProfilePictureKey),
                presenceTracker.IsOnline(UserId.From(friendship.FriendUserId))))
        ];
    }
}