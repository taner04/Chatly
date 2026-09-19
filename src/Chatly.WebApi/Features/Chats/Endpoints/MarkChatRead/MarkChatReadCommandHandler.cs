using Chatly.WebApi.Features.Chats.Exceptions;
using Chatly.WebApi.Features.Chats.Services;

namespace Chatly.WebApi.Features.Chats.Endpoints.MarkChatRead;

internal sealed class MarkChatReadCommandHandler(
    CurrentUserService currentUserService,
    ChatlyDbContext context,
    ChatAccessService chatAccessService) : ICommandHandler<MarkChatReadCommand>
{
    public async ValueTask<Unit> Handle(MarkChatReadCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        if (await chatAccessService.GetAsync(command.ChatId, userId, cancellationToken) is null)
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