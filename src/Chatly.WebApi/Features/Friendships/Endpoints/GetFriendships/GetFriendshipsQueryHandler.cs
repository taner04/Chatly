using Chatly.Contracts.Endpoints.Friendships.Results;
using Chatly.WebApi.Features.Hubs;

namespace Chatly.WebApi.Features.Friendships.Endpoints.GetFriendships;

internal sealed class GetFriendshipsQueryHandler(
    CurrentUserService currentUserService,
    ChatlyDbContext context,
    AzureBlobService blobService,
    OnlinePresenceTracker presenceTracker) : IQueryHandler<GetFriendshipsQuery, IReadOnlyList<GetFriendshipsResponse>>
{
    public async ValueTask<IReadOnlyList<GetFriendshipsResponse>> Handle(
        GetFriendshipsQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var friendships = await (
                from friendship in context.Friendships.AsNoTracking()
                where friendship.FirstUserId == userId || friendship.SecondUserId == userId
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
                    DirectChatId = (Guid?)chat.Id.Value,
                    FriendUserId = friend.Id.Value,
                    friend.Username,
                    friend.ProfilePictureKey
                })
            .ToListAsync(cancellationToken);

        return
        [
            .. friendships.Select(friendship => new GetFriendshipsResponse(
                friendship.FriendshipId,
                friendship.DirectChatId,
                friendship.FriendUserId,
                friendship.Username!,
                blobService.CreateReadUrl(friendship.ProfilePictureKey)?.ToString(),
                presenceTracker.IsOnline(UserId.From(friendship.FriendUserId))))
        ];
    }
}
