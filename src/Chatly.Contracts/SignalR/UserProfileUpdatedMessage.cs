namespace Chatly.Contracts.SignalR;

public sealed class UserProfileUpdatedMessage(
    Guid userId,
    string? username,
    string? profilePictureUrl)
    : NotificationMessage(NotificationType.UserProfileUpdated)
{
    public Guid UserId { get; } = userId;
    public string? Username { get; } = username;
    public string? ProfilePictureUrl { get; } = profilePictureUrl;
}