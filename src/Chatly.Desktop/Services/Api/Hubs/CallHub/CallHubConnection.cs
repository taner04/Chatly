using Chatly.Desktop.Options;
using Microsoft.Extensions.Options;

namespace Chatly.Desktop.Services.Api.Hubs.CallHub;

[SingletonService]
internal sealed class CallHubConnection(
    IOptions<WebApiClientOption> options,
    UserSessionContext sessionContext,
    ILogger<CallHubConnection> logger)
    : HubConnectionBase(options, sessionContext, logger)
{
    protected override string HubRoute => ApiRoutes.Hubs.Call;
}