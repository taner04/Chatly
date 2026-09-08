using Chatly.Contracts.Endpoints.Chats.Results;
using Chatly.WebApi.Features.Hubs;

namespace Chatly.WebApi.Features.Chats.Endpoints.GetChats;

internal sealed class GetChatsQueryHandler(
    CurrentUserService currentUserService,
    ChatlyDbContext context,
    AzureBlobService blobService,
    OnlinePresenceTracker presenceTracker) : IQueryHandler<GetChatsQuery, IReadOnlyList<GetChatsResponse>>
{
    public async ValueTask<IReadOnlyList<GetChatsResponse>> Handle(
        GetChatsQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        var chats = await (
                from chat in context.Chats.AsNoTracking()
                where chat.FirstUserId == userId || chat.SecondUserId == userId
                where context.Friendships.Any(friendship =>
                    friendship.FirstUserId == chat.FirstUserId &&
                    friendship.SecondUserId == chat.SecondUserId)
                let associatedUserId = chat.FirstUserId == userId ? chat.SecondUserId : chat.FirstUserId
                join associatedUser in context.Users.AsNoTracking() on associatedUserId equals associatedUser.Id
                where associatedUser.Username != null
                let lastReadAt = context.ChatReadStates
                    .Where(state => state.ChatId == chat.Id && state.UserId == userId)
                    .Select(state => (DateTimeOffset?)state.LastReadAt)
                    .FirstOrDefault()
                let unreadMessageCount = context.Messages.Count(message =>
                    message.ChatId == chat.Id &&
                    message.SenderUserId != userId &&
                    (lastReadAt == null || message.SentAt > lastReadAt))
                orderby associatedUser.Username, associatedUser.Id
                select new
                {
                    ChatId = chat.Id.Value,
                    AssociatedUserId = associatedUser.Id.Value,
                    associatedUser.Username,
                    associatedUser.ProfilePictureKey,
                    UnreadMessageCount = unreadMessageCount
                })
            .ToListAsync(cancellationToken);

        return
        [
            .. chats.Select(chat => new GetChatsResponse(
                chat.ChatId,
                chat.AssociatedUserId,
                chat.Username!,
                blobService.CreateReadUrl(chat.ProfilePictureKey)?.ToString(),
                presenceTracker.IsOnline(UserId.From(chat.AssociatedUserId)),
                chat.UnreadMessageCount))
        ];
    }
}
