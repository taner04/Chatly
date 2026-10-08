using System.Collections.Concurrent;
using System.Net.Http.Json;

namespace Chatly.WebApi.IntegrationTests.Infrastructure.Mocks.Identity;

public sealed class IdentityProviderHandlerMock : HttpMessageHandler
{
    private readonly ConcurrentQueue<string> _revokedSessionIds = new();
    private Exception? _failure;

    public IReadOnlyList<string> RevokedSessionIds => [.. _revokedSessionIds];

    public void FailWith(Exception failure) => _failure = failure;

    public void Reset()
    {
        _revokedSessionIds.Clear();
        _failure = null;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (_failure is not null)
        {
            return Task.FromException<HttpResponseMessage>(_failure);
        }

        var path = request.RequestUri!.AbsolutePath;

        return Task.FromResult(request.Method switch
        {
            _ when request.Method == HttpMethod.Post && path.EndsWith("/protocol/openid-connect/token") =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new { access_token = "identity-admin-token" })
                },
            _ when request.Method == HttpMethod.Delete && path.Contains("/sessions/") =>
                Revoke(path),
            _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        });
    }

    private HttpResponseMessage Revoke(string path)
    {
        _revokedSessionIds.Enqueue(Uri.UnescapeDataString(path[(path.LastIndexOf('/') + 1)..]));
        return new HttpResponseMessage(HttpStatusCode.NoContent);
    }
}