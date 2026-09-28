namespace Chatly.Contracts.Features.Users.Notifications;

public sealed class UserProfileUpdatedNotification(
    Guid userId,
    string? username,
    string? profilePictureUrl)
    : NotificationMessage
{
    public Guid UserId { get; } = userId;
    public string? Username { get; } = username;
    public string? ProfilePictureUrl { get; } = profilePictureUrl;
}