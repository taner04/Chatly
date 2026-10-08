using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.Options;
using Chatly.Desktop.Services.Authentication;
using Microsoft.Extensions.Options;

namespace Chatly.Desktop.Services.Api.Hubs.NotificationHub;

[SingletonService]
internal sealed class NotificationHubConnection(
    IOptions<WebApiClientOption> options,
    AuthenticationService authenticationService,
    AppSettings appSettings,
    ILogger<NotificationHubConnection> logger)
    : HubConnectionBase(options, authenticationService, appSettings, logger)
{
    protected override string HubRoute => ApiRoutes.Hubs.Notification;
}