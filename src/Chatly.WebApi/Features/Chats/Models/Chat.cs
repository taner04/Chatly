using Chatly.WebApi.Common.Shared.Models;
using Chatly.WebApi.Features.Messages.Models;
using Chatly.WebApi.Features.Users.Models;
using Vogen;

namespace Chatly.WebApi.Features.Chats.Models;

[ValueObject<Guid>]
public readonly partial struct ChatId
{
    private static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("ChatId must be set to a non-default value.");
}

public abstract class Chat : Entity<ChatId>
{
    protected Chat()
    {
    }

    protected Chat(ChatId id)
    {
        Id = id;
    }

    public ICollection<ChatMember> Members { get; } = [];
    public ICollection<User> Users { get; } = [];
    public ICollection<Message> Messages { get; private set; } = [];

    public void AddMember(UserId userId, ChatMemberRole role = ChatMemberRole.Member)
    {
        if (Members.Any(member => member.UserId == userId))
        {
            return;
        }

        Members.Add(ChatMember.Create(Id, userId, role));
    }
}
