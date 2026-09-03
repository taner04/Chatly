namespace Chatly.Contracts.SignalR;

public sealed class TypingStatusChangedMessage(
    Guid chatId,
    bool isTyping) : NotificationMessage(NotificationType.TypingStatusChanged)
{
    public Guid ChatId { get; } = chatId;

    public bool IsTyping { get; } = isTyping;
}