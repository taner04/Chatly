namespace Chatly.Contracts.Features.Hubs.Notifications;

public sealed class TypingStatusChangedNotification(
    Guid chatId,
    bool isTyping) : Notification
{
    public Guid ChatId { get; } = chatId;

    public bool IsTyping { get; } = isTyping;
}