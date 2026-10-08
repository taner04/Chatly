using Chatly.Contracts.Features.DeviceSessions.Notifications;
using Chatly.WebApi.Features.DeviceSessions.Services;

namespace Chatly.WebApi.Features.DeviceSessions.Endpoints.RevokeCurrentDeviceSession;

internal sealed class RevokeCurrentDeviceSessionCommandHandler(
    DeviceSessionService deviceSessionService,
    ChatlyDbContext context,
    CurrentUserService currentUserService,
    IdentitySessionClient identitySessionClient,
    NotificationPublisher notificationPublisher) : ICommandHandler<RevokeCurrentDeviceSessionCommand>
{
    public async ValueTask<Unit> Handle(RevokeCurrentDeviceSessionCommand command, CancellationToken cancellationToken)
    {
        var session = await deviceSessionService.RevokeCurrentAsync(cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        await notificationPublisher.PublishAsync(currentUserService.UserId, new DeviceSessionsChangedNotification());
        await identitySessionClient.RevokeAsync([session?.IdentitySessionId], cancellationToken);

        return Unit.Value;
    }
}