using Chatly.WebApi.Features.DeviceSessions.Services;

namespace Chatly.WebApi.Common.Behaviours;

internal sealed class DeviceSessionBehaviour<TMessage, TResponse>(DeviceSessionService deviceSessionService)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        _ = await deviceSessionService.GetOrCreateCurrentActiveAsync(cancellationToken);

        return await next(message, cancellationToken);
    }
}