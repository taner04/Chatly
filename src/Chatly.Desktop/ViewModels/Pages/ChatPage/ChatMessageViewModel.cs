namespace Chatly.Desktop.ViewModels.Pages.ChatPage;

public sealed class ChatMessageViewModel(
    Guid messageId,
    Guid senderUserId,
    string content,
    DateTimeOffset sentAt,
    bool isOwnMessage)
{
    public Guid MessageId { get; } = messageId;

    public Guid SenderUserId { get; } = senderUserId;

    public string Content { get; } = content;

    public DateTimeOffset SentAt { get; } = sentAt;

    public bool IsOwnMessage { get; } = isOwnMessage;

    public string Time => SentAt.ToLocalTime().ToString("t");
}