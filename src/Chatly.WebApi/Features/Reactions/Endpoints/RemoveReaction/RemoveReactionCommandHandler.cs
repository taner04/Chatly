using Chatly.Contracts.Features.Reactions.Notifications;
using Chatly.WebApi.Features.Chats.Services;
using Chatly.WebApi.Features.Reactions.Models;

namespace Chatly.WebApi.Features.Reactions.Endpoints.RemoveReaction;

internal sealed class RemoveReactionCommandHandler(
    ChatlyDbContext context,
    CurrentUserService userService,
    ChatAccessService chatAccessService,
    NotificationPublisher notificationPublisher) : ICommandHandler<RemoveReactionCommand>
{
    public async ValueTask<Unit> Handle(RemoveReactionCommand command, CancellationToken cancellationToken)
    {
        var userId = userService.GetCurrentUserId();

        var reaction = await context.Reactions
                           .FirstOrDefaultAsync(
                               reaction => reaction.Id == command.ReactionId && reaction.UserId == userId,
                               cancellationToken)
                       ?? throw new EntityNotFoundException<Reaction>(command.ReactionId.Value);

        var chat = await chatAccessService.GetForMessageAsync(
                       reaction.MessageId,
                       userId,
                       cancellationToken)
                   ?? throw new EntityNotFoundException<Message>(reaction.MessageId.Value);

        context.Reactions.Remove(reaction);
        await context.SaveChangesAsync(cancellationToken);

        await notificationPublisher.PublishAsync(
            [userId, chat.OtherParticipantUserId],
            new ReactionChangedNotification(
                chat.ChatId.Value,
                reaction.MessageId.Value,
                reaction.Id.Value,
                reaction.UserId.Value,
                reaction.Type,
                true));

        return Unit.Value;
    }
}