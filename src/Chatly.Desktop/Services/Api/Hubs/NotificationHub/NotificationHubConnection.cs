using Chatly.Desktop.Options;
using Microsoft.Extensions.Options;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub;

[SingletonService]
internal sealed class NotificationHubConnection(
    IOptions<WebApiClientOption> options,
    UserSessionContext sessionContext,
    ILogger<NotificationHubConnection> logger)
    : HubConnectionBase(options, sessionContext, logger)
{
    protected override string HubRoute => ApiRoutes.Hubs.Notification;
}