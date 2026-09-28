namespace Chatly.Contracts.Features.Reactions.Notifications;

public sealed class ReactionChangedNotification(
    Guid chatId,
    Guid messageId,
    Guid reactionId,
    Guid userId,
    ReactionType reactionType,
    bool isRemoved) : NotificationMessage
{
    public Guid ChatId { get; } = chatId;

    public Guid MessageId { get; } = messageId;

    public Guid ReactionId { get; } = reactionId;

    public Guid UserId { get; } = userId;

    public ReactionType ReactionType { get; } = reactionType;

    public bool IsRemoved { get; } = isRemoved;
}