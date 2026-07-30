using Chatly.WebApi.Features.Users.Models;

namespace Chatly.WebApi.Features.Chats.Models;

public sealed class DirectChat : Chat
{
    public const int ParticipantKeyLength = 65;

    private DirectChat()
    {
    }

    public DirectChat(UserId firstUserId, UserId secondUserId)
        : base(ChatId.From(Guid.CreateVersion7()))
    {
        if (firstUserId == secondUserId)
        {
            throw new ArgumentException(
                "A direct chat requires two different users.",
                nameof(secondUserId));
        }

        ParticipantKey = CreateParticipantKey(firstUserId, secondUserId);
        AddMember(firstUserId);
        AddMember(secondUserId);
    }

    public string ParticipantKey { get; private init; } = null!;

    public static string CreateParticipantKey(UserId firstUserId, UserId secondUserId)
    {
        var first = firstUserId.Value;
        var second = secondUserId.Value;

        return first.CompareTo(second) < 0
            ? $"{first:N}:{second:N}"
            : $"{second:N}:{first:N}";
    }
}