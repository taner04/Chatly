using Chatly.WebApi.Features.DeviceSessions.Services;

namespace Chatly.WebApi.Features.DeviceSessions.Endpoints.RevokeOtherDeviceSessions;

internal sealed class RevokeOtherDeviceSessionsCommandHandler(
    DeviceSessionService deviceSessionService,
    ChatlyDbContext context,
    IdentitySessionClient identitySessionClient) : ICommandHandler<RevokeOtherDeviceSessionsCommand>
{
    public async ValueTask<Unit> Handle(RevokeOtherDeviceSessionsCommand command, CancellationToken cancellationToken)
    {
        var sessions = await deviceSessionService.RevokeOthersAsync(cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        await deviceSessionService.PublishRevokedAsync(sessions);
        await identitySessionClient.RevokeAsync(
            sessions.Select(session => session.IdentitySessionId),
            cancellationToken);

        return Unit.Value;
    }
}