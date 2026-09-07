namespace Chatly.Desktop.Models;

public sealed class DirectChat
{
    public required Guid Id { get; init; }

    public required User User { get; init; }

    public int UnreadMessageCount { get; init; }
}