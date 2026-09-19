namespace Chatly.Contracts.Features.Messages.Models;

public sealed record MessageContract(
    Guid MessageId,
    Guid ChatId,
    Guid SenderUserId,
    string Content,
    DateTimeOffset SentAt,
    bool IsDeleted,
    IReadOnlyList<MessageAttachmentContract> Attachments,
    IReadOnlyList<MessageReactionContract> Reactions);