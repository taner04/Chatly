using Chatly.WebApi.Common.Shared.Models;
using Chatly.WebApi.Features.Messages.Models;
using Vogen;

namespace Chatly.WebApi.Features.Chats.Models;

[ValueObject<Guid>]
public readonly partial struct ChatId
{
    private static Validation Validate(Guid value)
    {
        return value.Validate<ChatId>();
    }
}

public sealed class Chat : UserPairEntity<ChatId>
{
    private Chat()
    {
    }

    public Chat(UserId firstUserId, UserId secondUserId)
        : base(ChatId.From(Guid.CreateVersion7()), firstUserId, secondUserId)
    {
    }

    public ICollection<Message> Messages { get; init; } = [];
}