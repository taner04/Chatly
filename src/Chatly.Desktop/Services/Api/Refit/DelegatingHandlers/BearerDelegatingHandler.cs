using System.Net.Http;
using System.Net.Http.Headers;
using Chatly.Desktop.Services.Authentication;

namespace Chatly.Desktop.Services.Api.Refit.DelegatingHandlers;

[TransientService]
internal sealed class BearerDelegatingHandler(AuthenticationService authenticationService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await authenticationService.GetAccessTokenAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}