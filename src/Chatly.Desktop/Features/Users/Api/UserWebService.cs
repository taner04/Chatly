using System;
using System.Threading;
using System.Threading.Tasks;
using Chatly.Contracts.Users.Requests;
using Chatly.Contracts.Users.Results;
using Chatly.Desktop.Common.Api;
using Chatly.Desktop.Common.Api.Results;

namespace Chatly.Desktop.Features.Users.Api;

public sealed class UserWebService(IUserEndpoint userEndpoint)
{
    public Task<WebClientResult<CurrentUserResponse>> GetCurrentUserAsync(
        CancellationToken cancellationToken = default) =>
        HttpOrchestrator.ExecuteAsync(
            () => userEndpoint.GetCurrentUserAsync(cancellationToken),
            cancellationToken);

    public Task<WebClientResult<CurrentUserResponse>> UpdateUsernameAsync(
        UpdateUsernameRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HttpOrchestrator.ExecuteAsync(
            () => userEndpoint.UpdateUsernameAsync(request, cancellationToken),
            cancellationToken);
    }
}