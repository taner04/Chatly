using Chatly.WebApi.Common.Shared.Models;
using Chatly.WebApi.Features.Users.Models;
using Vogen;

namespace Chatly.WebApi.Features.Chats.Models;

[ValueObject<Guid>]
public readonly partial struct ChatMemberId
{
    private static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("ChatMemberId must be set to a non-default value.");
}

public sealed class ChatMember : Entity<ChatMemberId>
{
    private ChatMember()
    {
    }

    private ChatMember(ChatId chatId, UserId userId, ChatMemberRole role)
    {
        Id = ChatMemberId.From(Guid.CreateVersion7());
        ChatId = chatId;
        UserId = userId;
        Role = role;
    }

    public ChatId ChatId { get; private init; }
    public Chat Chat { get; private init; } = null!;

    public UserId UserId { get; private init; }
    public User User { get; private init; } = null!;

    public ChatMemberRole Role { get; private set; }

    internal static ChatMember Create(ChatId chatId, UserId userId, ChatMemberRole role) =>
        new(chatId, userId, role);
}

public enum ChatMemberRole
{
    Member,
    Admin,
    Owner
}