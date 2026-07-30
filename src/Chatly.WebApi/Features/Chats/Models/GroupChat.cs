using Chatly.WebApi.Common.Shared.Guards;
using Chatly.WebApi.Features.Users.Models;

namespace Chatly.WebApi.Features.Chats.Models;

public sealed class GroupChat : Chat
{
    public const int MaxNameLength = 200;

    private GroupChat()
    {
    }

    public GroupChat(string name, UserId ownerId)
        : base(ChatId.From(Guid.CreateVersion7()))
    {
        Guard.Against.NullOrEmpty<GroupChat>(name);
        Guard.Against.LengthBetween<GroupChat>(name, 1, MaxNameLength);

        Name = name;
        AddMember(ownerId, ChatMemberRole.Owner);
    }

    public string Name { get; private set; } = null!;
}