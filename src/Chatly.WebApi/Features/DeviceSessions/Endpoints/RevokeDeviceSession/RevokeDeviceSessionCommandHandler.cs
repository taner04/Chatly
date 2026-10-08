using Chatly.WebApi.Features.DeviceSessions.Services;

namespace Chatly.WebApi.Features.DeviceSessions.Endpoints.RevokeDeviceSession;

internal sealed class RevokeDeviceSessionCommandHandler(
    DeviceSessionService deviceSessionService,
    ChatlyDbContext context,
    IdentitySessionClient identitySessionClient) : ICommandHandler<RevokeDeviceSessionCommand>
{
    public async ValueTask<Unit> Handle(RevokeDeviceSessionCommand command, CancellationToken cancellationToken)
    {
        var session = await deviceSessionService.RevokeAsync(command.SessionId, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        await deviceSessionService.PublishRevokedAsync([session]);
        await identitySessionClient.RevokeAsync([session.IdentitySessionId], cancellationToken);

        return Unit.Value;
    }
}