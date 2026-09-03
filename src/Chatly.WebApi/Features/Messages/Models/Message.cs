using Chatly.WebApi.Common.Shared.Models;
using Chatly.WebApi.Features.Chats.Models;
using Vogen;

namespace Chatly.WebApi.Features.Messages.Models;

[ValueObject<Guid>]
public readonly partial struct MessageId
{
    private static Validation Validate(Guid value)
    {
        return value.Validate<MessageId>();
    }
}

public sealed class Message : Entity<MessageId>
{
    public const int MaxContentLength = 4_000;

    private Message()
    {
    }

    public Message(ChatId chatId, UserId senderUserId, string content)
        : base(MessageId.From(Guid.CreateVersion7()))
    {
        ChatId = chatId;
        SenderUserId = senderUserId;
        Content = content;
    }

    public ChatId ChatId { get; init; }

    public UserId SenderUserId { get; init; }

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;

    public Chat Chat { get; init; } = null!;

    public User SenderUser { get; init; } = null!;
}