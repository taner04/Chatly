namespace Chatly.WebApi.Features.Chats.Models;

[ValueObject<Guid>]
public readonly partial struct ChatReadStateId : IGuidEntityId<ChatReadStateId>
{
    private static Validation Validate(Guid value) => value.Validate<ChatReadStateId>();
}

public sealed class ChatReadState : Entity<ChatReadStateId>
{
    [UsedImplicitly]
    private ChatReadState()
    {
    }

    public ChatReadState(ChatId chatId, UserId userId)
    {
        ChatId = chatId;
        UserId = userId;
        LastReadAt = DateTimeOffset.UtcNow;
    }

    public ChatId ChatId { get; private init; }

    public UserId UserId { get; private init; }

    public DateTimeOffset LastReadAt { get; set; }
}