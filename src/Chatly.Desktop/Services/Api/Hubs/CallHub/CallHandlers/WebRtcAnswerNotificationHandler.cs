using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;
using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Services.Calls;

namespace Chatly.Desktop.Services.Api.Hubs.CallHub.CallHandlers;

[SingletonService(typeof(IHubMessageHandler<Call>))]
internal sealed class WebRtcAnswerNotificationHandler(
    CallCoordinator coordinator,
    ILogger<CallHandler<WebRtcAnswerNotification>> logger)
    : CallHandler<WebRtcAnswerNotification>(coordinator, logger);
