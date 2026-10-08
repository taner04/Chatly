using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.Options;
using Chatly.Desktop.Services.Authentication;
using Microsoft.Extensions.Options;

namespace Chatly.Desktop.Services.Api.Hubs.CallHub;

[SingletonService]
internal sealed class CallHubConnection(
    IOptions<WebApiClientOption> options,
    AuthenticationService authenticationService,
    AppSettings appSettings,
    ILogger<CallHubConnection> logger)
    : HubConnectionBase(options, authenticationService, appSettings, logger)
{
    protected override string HubRoute => ApiRoutes.Hubs.Call;
}