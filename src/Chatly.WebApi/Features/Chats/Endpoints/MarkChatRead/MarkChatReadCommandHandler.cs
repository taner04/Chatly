using Chatly.WebApi.Features.Chats.Models;

namespace Chatly.WebApi.Features.Chats.Endpoints.MarkChatRead;

public sealed class MarkChatReadCommandHandler(
    CurrentUserService currentUserService,
    ChatlyDbContext context) : ICommandHandler<MarkChatReadCommand>
{
    public async ValueTask<Unit> Handle(MarkChatReadCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        var canAccessChat = await context.Chats
            .AsNoTracking()
            .AnyAsync(chat =>
                    chat.Id == command.ChatId &&
                    (chat.FirstUserId == userId || chat.SecondUserId == userId) &&
                    context.Friendships.Any(friendship =>
                        friendship.FirstUserId == chat.FirstUserId &&
                        friendship.SecondUserId == chat.SecondUserId),
                cancellationToken);

        if (!canAccessChat)
        {
            throw new EntityNotFoundException<Chat>(command.ChatId.Value);
        }

        var readAt = DateTimeOffset.UtcNow;
        await context.Database.ExecuteSqlInterpolatedAsync($"""
                                                            INSERT INTO "ChatReadStates" ("ChatId", "UserId", "LastReadAt")
                                                            VALUES ({command.ChatId.Value}, {userId.Value}, {readAt})
                                                            ON CONFLICT ("ChatId", "UserId") DO UPDATE
                                                            SET "LastReadAt" = GREATEST("ChatReadStates"."LastReadAt", EXCLUDED."LastReadAt")
                                                            """, cancellationToken);

        return Unit.Value;
    }
}