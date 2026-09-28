namespace Chatly.Contracts.Features.Hubs.Notifications.NotificationHubServer;

public sealed class TypingStatusChangedNotification(
    Guid chatId,
    bool isTyping) : NotificationMessage
{
    public Guid ChatId { get; } = chatId;

    public bool IsTyping { get; } = isTyping;
}