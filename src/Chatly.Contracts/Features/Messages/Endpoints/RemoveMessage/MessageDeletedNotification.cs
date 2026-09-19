namespace Chatly.Contracts.Features.Messages.Endpoints.RemoveMessage;

public sealed class MessageDeletedNotification(
    Guid chatId,
    Guid messageId) : Notification
{
    public Guid ChatId { get; } = chatId;
    public Guid MessageId { get; } = messageId;
}