namespace Chatly.WebApi.Features.Chats.Models;

internal sealed class ChatReadState
{
    private ChatReadState()
    {
    }

    public ChatId ChatId { get; private init; }

    public UserId UserId { get; private init; }

    public DateTimeOffset LastReadAt { get; private set; }
}
