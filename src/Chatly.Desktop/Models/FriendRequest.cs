namespace Chatly.Desktop.Models;

public sealed class FriendRequest : IIdentifiable
{
    public Guid Id { get; init; }

    public string SenderUsername { get; init; } = string.Empty;

    public string? ProfilePictureUrl { get; init; }
}
