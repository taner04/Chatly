using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Services.Calls;

namespace Chatly.Desktop.Services.Api.Hubs.CallHub.CallHandlers;

[SingletonService(typeof(IHubMessageHandler<Call>))]
internal sealed class CallEndedNotificationHandler(
    CallCoordinator coordinator,
    ILogger<CallHandler<CallEndedNotification>> logger)
    : CallHandler<CallEndedNotification>(coordinator, logger);
