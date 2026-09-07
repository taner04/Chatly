namespace Chatly.Desktop.Models;

public sealed class FriendRequest
{
    public Guid Id { get; init; }

    public string SenderUsername { get; init; } = string.Empty;

    public string? ProfilePictureUrl { get; init; }
}