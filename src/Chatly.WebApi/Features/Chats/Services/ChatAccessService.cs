namespace Chatly.WebApi.Features.Chats.Services;

[ScopedService]
public sealed class ChatAccessService(ChatlyDbContext context)
{
    internal Task<ChatAccess?> GetAsync(
        ChatId chatId,
        UserId userId,
        CancellationToken cancellationToken)
    {
        return AccessibleChats(userId)
            .Where(chat => chat.Id == chatId)
            .Select(chat => new ChatAccess(
                chat.Id,
                chat.FirstUserId == userId ? chat.SecondUserId : chat.FirstUserId))
            .SingleOrDefaultAsync(cancellationToken);
    }

    internal Task<ChatAccess?> GetForMessageAsync(
        MessageId messageId,
        UserId userId,
        CancellationToken cancellationToken)
    {
        return AccessibleChats(userId)
            .Where(chat => context.Messages.Any(message =>
                message.Id == messageId &&
                message.ChatId == chat.Id &&
                !message.IsDeleted))
            .Select(chat => new ChatAccess(
                chat.Id,
                chat.FirstUserId == userId ? chat.SecondUserId : chat.FirstUserId))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private IQueryable<Chat> AccessibleChats(UserId userId)
    {
        return context.Chats
            .AsNoTracking()
            .ForUser(userId)
            .Where(chat => context.Friendships.Any(friendship =>
                friendship.FirstUserId == chat.FirstUserId &&
                friendship.SecondUserId == chat.SecondUserId));
    }
}