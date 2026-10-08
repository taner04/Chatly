using Chatly.Contracts.Features.Reactions.Models;
using Chatly.Contracts.Features.Reactions.Notifications;
using Chatly.WebApi.Features.Chats.Services;
using Chatly.WebApi.Features.Reactions.Models;

namespace Chatly.WebApi.Features.Reactions.Endpoints.SetReaction;

internal sealed class SetReactionCommandHandler(
    ChatlyDbContext context,
    CurrentUserService userService,
    ChatAccessService chatAccessService,
    NotificationPublisher notificationPublisher) : ICommandHandler<SetReactionCommand, MessageReactionContract>
{
    public async ValueTask<MessageReactionContract> Handle(
        SetReactionCommand command,
        CancellationToken cancellationToken)
    {
        var userId = userService.UserId;

        var chat = await chatAccessService.GetForMessageAsync(
                       command.MessageId,
                       userId,
                       cancellationToken)
                   ?? throw new EntityNotFoundException<Message>(command.MessageId.Value);

        var reaction = await context.Reactions
            .FirstOrDefaultAsync(
                reaction => reaction.MessageId == command.MessageId && reaction.UserId == userId,
                cancellationToken);

        var isNew = reaction is null;
        if (reaction is null)
        {
            reaction = new Reaction(userId, command.MessageId, command.ReactionType);
            context.Reactions.Add(reaction);
        }
        else
        {
            reaction.Type = command.ReactionType;
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (isNew && exception.IsUniqueViolation())
        {
            context.Entry(reaction).State = EntityState.Detached;
            reaction = await context.Reactions.SingleAsync(
                candidate => candidate.MessageId == command.MessageId && candidate.UserId == userId,
                cancellationToken);
            reaction.Type = command.ReactionType;
            await context.SaveChangesAsync(cancellationToken);
        }

        var response = new MessageReactionContract(
            reaction.Id.Value,
            reaction.UserId.Value,
            reaction.Type);

        await notificationPublisher.PublishAsync(
            [userId, chat.OtherParticipantUserId],
            new ReactionChangedNotification(
                chat.ChatId.Value,
                command.MessageId.Value,
                reaction.Id.Value,
                reaction.UserId.Value,
                reaction.Type,
                false));

        return response;
    }
}