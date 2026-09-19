using Chatly.Contracts.Features.Users.Notifications;

namespace Chatly.WebApi.Features.Users.Services.Profiles;

[ScopedService]
internal sealed class UserProfileUpdateNotifier(
    ChatlyDbContext context,
    NotificationPublisher notificationPublisher)
{
    internal async Task PublishAsync(
        CurrentUserResponse profile,
        CancellationToken cancellationToken)
    {
        var userId = UserId.From(profile.UserId);
        var recipientIds = await context.Friendships
            .AsNoTracking()
            .ForUser(userId)
            .SelectOtherUserId(userId)
            .ToListAsync(cancellationToken);

        recipientIds.Add(userId);

        await notificationPublisher.PublishAsync(recipientIds, new UserProfileUpdatedNotification(
            profile.UserId,
            profile.Username,
            profile.ProfilePictureUrl));
    }
}