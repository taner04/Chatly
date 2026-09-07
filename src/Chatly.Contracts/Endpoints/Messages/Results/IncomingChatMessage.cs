using Chatly.Contracts.SignalR;

namespace Chatly.Contracts.Endpoints.Messages.Results;

public sealed class IncomingChatMessage(
    Guid messageId,
    Guid chatId,
    Guid senderUserId,
    string content,
    DateTimeOffset sentAt) : NotificationMessage(NotificationType.IncomingMessage)
{
    public Guid MessageId { get; } = messageId;
    public Guid ChatId { get; } = chatId;
    public Guid SenderUserId { get; } = senderUserId;
    public string Content { get; } = content;
    public DateTimeOffset SentAt { get; } = sentAt;
}