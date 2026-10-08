using Chatly.WebApi.Features.MessageAttachments.Models;
using Chatly.WebApi.Features.Reactions.Models;

namespace Chatly.WebApi.Features.Messages.Models;

[ValueObject<Guid>]
public readonly partial struct MessageId : IGuidEntityId<MessageId>
{
    private static Validation Validate(Guid value) => value.Validate<MessageId>();
}

public sealed class Message : Entity<MessageId>
{
    internal const int MaxContentLength = 4_000;

    [UsedImplicitly]
    private Message()
    {
    }

    public Message(ChatId chatId, UserId senderUserId, string content)
    {
        ChatId = chatId;
        SenderUserId = senderUserId;
        Content = content;
        SentAt = DateTimeOffset.UtcNow;
        IsDeleted = false;
    }

    public ChatId ChatId { get; init; }

    public UserId SenderUserId { get; init; }

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset SentAt { get; init; }

    public Chat Chat { get; init; } = null!;

    public User SenderUser { get; init; } = null!;

    public bool IsDeleted { get; set; }

    public ICollection<Reaction> Reactions { get; init; } = [];

    public ICollection<MessageAttachment> Attachments { get; init; } = [];
}