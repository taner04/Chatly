using Chatly.WebApi.Features.Chats.Exceptions;
using Chatly.WebApi.Features.Chats.Models;

namespace Chatly.WebApi.Features.Chats.Endpoints.MarkChatRead;

internal sealed class MarkChatReadCommandHandler(
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
            throw new ChatAccessDeniedException(command.ChatId);
        }

        var readState = await context.ChatReadStates
            .SingleOrDefaultAsync(state =>
                    state.ChatId == command.ChatId &&
                    state.UserId == userId,
                cancellationToken);
        
        if (readState is null)
        {
            context.ChatReadStates.Add(new ChatReadState(command.ChatId, userId));
        }
        else
        {
            readState.LastReadAt = DateTimeOffset.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
