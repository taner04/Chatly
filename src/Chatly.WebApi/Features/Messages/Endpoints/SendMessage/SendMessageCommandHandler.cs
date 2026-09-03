using Chatly.Contracts.Endpoints.Messages.Results;
using Chatly.WebApi.Features.Chats.Models;
using Chatly.WebApi.Features.Messages.Models;

namespace Chatly.WebApi.Features.Messages.Endpoints.SendMessage;

public sealed class SendMessageCommandHandler(
    CurrentUserService currentUserService,
    ChatlyDbContext context,
    NotificationPublisher notificationPublisher) : ICommandHandler<SendMessageCommand, SendMessageResponse>
{
    public async ValueTask<SendMessageResponse> Handle(
        SendMessageCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var chat = await context.Chats
                       .AsNoTracking()
                       .Where(chat => chat.Id == command.ChatId)
                       .Where(chat => context.Friendships.Any(friendship =>
                           friendship.FirstUserId == chat.FirstUserId &&
                           friendship.SecondUserId == chat.SecondUserId))
                       .Select(chat => new
                       {
                           chat.Id,
                           chat.FirstUserId,
                           chat.SecondUserId
                       })
                       .FirstOrDefaultAsync(cancellationToken)
                   ?? throw new EntityNotFoundException<Chat>(command.ChatId.Value);

        var receiverUserId = chat.FirstUserId == userId
            ? chat.SecondUserId
            : chat.SecondUserId == userId
                ? chat.FirstUserId
                : throw new EntityNotFoundException<Chat>(command.ChatId.Value);

        var message = new Message(chat.Id, userId, command.Content.Trim());
        context.Messages.Add(message);

        await context.SaveChangesAsync(cancellationToken);

        await notificationPublisher.PublishAsync(receiverUserId, new IncomingChatMessage(
            message.Id.Value,
            message.ChatId.Value,
            message.SenderUserId.Value,
            message.Content,
            message.SentAt));

        return new SendMessageResponse(
            message.Id.Value,
            message.ChatId.Value,
            message.SenderUserId.Value,
            message.Content,
            message.SentAt);
    }
}