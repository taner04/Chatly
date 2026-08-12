using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Chatly.Desktop.Features.Authentication.Services;
using Chatly.Desktop.Features.Users;

namespace Chatly.Desktop.Common.Api;

internal sealed class BearerDelegatingHandler(UserSessionContext context) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = context.AccessToken;

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}