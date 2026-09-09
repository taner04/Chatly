using Chatly.WebApi.Common.Shared.Models;
using Vogen;

namespace Chatly.WebApi.Features.Chats.Models;

[ValueObject<Guid>]
public readonly partial struct ChatReadStateId
{
    private static Validation Validate(Guid value)
    {
        return value.Validate<ChatReadStateId>();
    }
}

public sealed class ChatReadState : Entity<ChatReadStateId>
{
    private ChatReadState()
    {
    }

    public ChatReadState(ChatId chatId, UserId userId)
        : base(ChatReadStateId.From(Guid.CreateVersion7()))
    {
        ChatId = chatId;
        UserId = userId;
        LastReadAt = DateTimeOffset.UtcNow;
    }

    public ChatId ChatId { get; private init; }

    public UserId UserId { get; private init; }

    public DateTimeOffset LastReadAt { get; set; }
}
