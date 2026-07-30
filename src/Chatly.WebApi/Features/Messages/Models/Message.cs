using Chatly.WebApi.Common.Shared.Guards;
using Chatly.WebApi.Common.Shared.Models;
using Chatly.WebApi.Features.Chats.Models;
using Chatly.WebApi.Features.Users.Models;
using Vogen;
using ChatId = Chatly.WebApi.Features.Chats.Models.ChatId;

namespace Chatly.WebApi.Features.Messages.Models;

[ValueObject<Guid>]
public readonly partial struct MessageId
{
    private static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("MessageId must be set to a non-default value.");
}

public sealed class Message : Entity<MessageId>
{
    private Message()
    {
    }

    public Message(ChatId chatId, UserId senderId, string content)
    {
        Guard.Against.NullOrEmpty<Message>(content);

        Id = MessageId.From(Guid.CreateVersion7());
        ChatId = chatId;
        SenderId = senderId;
        Content = content;
    }

    public ChatId ChatId { get; private init; }
    public Chat Chat { get; private init; } = null!;

    public UserId SenderId { get; private init; }
    public User Sender { get; private init; } = null!;

    public string Content { get; private init; } = null!;
}