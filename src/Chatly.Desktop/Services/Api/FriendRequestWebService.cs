using System;
using System.Threading;
using System.Threading.Tasks;
using Chatly.Contracts.FriendRequests.Requests;
using Chatly.Desktop.Services.Api.Refit;
using Chatly.Desktop.Services.Api.Results;

namespace Chatly.Desktop.Services.Api;

public sealed class FriendRequestWebService(IFriendRequestEndpoint friendRequestEndpoint)
{
    public Task<WebClientResult> SendFriendRequestAsync(
        SendFriendRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HttpOrchestrator.ExecuteAsync(
            () => friendRequestEndpoint.SendFriendRequestAsync(request, cancellationToken),
            cancellationToken);
    }
}