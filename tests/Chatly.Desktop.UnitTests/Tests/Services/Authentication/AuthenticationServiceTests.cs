using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Chatly.Desktop.Abstraction.Authentication;
using Chatly.Desktop.Models.UserSession;
using Chatly.Desktop.Options;
using Chatly.Desktop.Services.Authentication;
using Chatly.Desktop.UnitTests.Infrastructure;
using Duende.IdentityModel.Jwk;
using Duende.IdentityModel.OidcClient;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chatly.Desktop.UnitTests.Tests.Services.Authentication;

public sealed class AuthenticationServiceTests : TestBase
{
    private const string Authority = "https://identity.test/realms/chatly";
    private const string StoredRefreshToken = "stored-refresh-token";
    private readonly AuthenticationService _service;
    private readonly UserSessionContext _sessionContext = new(new UserRegistry());

    private readonly TokenEndpointHandler _tokenEndpoint = new();
    private readonly ISecureTokenStore _tokenStore = Substitute.For<ISecureTokenStore>();
    private bool _revoked;

    public AuthenticationServiceTests()
    {
        _tokenStore.TryReadRefreshTokenAsync(Arg.Any<CancellationToken>()).Returns(StoredRefreshToken);
        _sessionContext.DeviceSessionRevoked += (_, _) => _revoked = true;

        var options = new OidcOption
        {
            Authority = Authority,
            ClientId = "chatly-desktop",
            Scope = "openid offline_access",
            RedirectUri = "http://127.0.0.1/callback"
        };
        var client = new OidcClient(new OidcClientOptions
        {
            Authority = Authority,
            ClientId = options.ClientId,
            Scope = options.Scope,
            RedirectUri = options.RedirectUri,
            BackchannelHandler = _tokenEndpoint,
            ProviderInformation = new ProviderInformation
            {
                IssuerName = Authority,
                AuthorizeEndpoint = $"{Authority}/protocol/openid-connect/auth",
                TokenEndpoint = $"{Authority}/protocol/openid-connect/token",
                KeySet = new JsonWebKeySet()
            }
        });
        _service = new AuthenticationService(
            options,
            client,
            _tokenStore,
            _sessionContext,
            NullLogger<AuthenticationService>.Instance);
    }

    [Fact]
    public async Task GetAccessTokenAsync_Should_ReturnCachedToken_When_TokenIsNotAboutToExpire()
    {
        _tokenEndpoint.Respond(Token("access-1", TimeSpan.FromHours(1)));
        await _service.AuthenticateAsync(CurrentCancellationToken);

        var accessToken = await _service.GetAccessTokenAsync(CurrentCancellationToken);

        accessToken.Should().Be("access-1");
        _tokenEndpoint.RequestCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAccessTokenAsync_Should_RefreshAndStoreRotatedToken_When_TokenIsAboutToExpire()
    {
        await AuthenticateWithExpiringTokenAsync();
        _tokenEndpoint.Respond(Token("access-2", TimeSpan.FromHours(1), "rotated-refresh-token"));

        var accessToken = await _service.GetAccessTokenAsync(CurrentCancellationToken);

        accessToken.Should().Be("access-2");
        _tokenEndpoint.RequestCount.Should().Be(2);
        await _tokenStore.Received(1)
            .SaveRefreshTokenAsync("rotated-refresh-token", Arg.Any<CancellationToken>());
        _revoked.Should().BeFalse();
    }

    [Fact]
    public async Task GetAccessTokenAsync_Should_EndSession_When_RefreshTokenIsRejected()
    {
        await AuthenticateWithExpiringTokenAsync();
        _tokenEndpoint.Respond(Error(HttpStatusCode.BadRequest, "invalid_grant"));

        var accessToken = await _service.GetAccessTokenAsync(CurrentCancellationToken);
        var nextAccessToken = await _service.GetAccessTokenAsync(CurrentCancellationToken);

        accessToken.Should().BeNull();
        nextAccessToken.Should().BeNull();
        _revoked.Should().BeTrue();
        _tokenEndpoint.RequestCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAccessTokenAsync_Should_ThrowAndKeepSession_When_RefreshFailsForOtherReason()
    {
        await AuthenticateWithExpiringTokenAsync();
        _tokenEndpoint.Respond(Error(HttpStatusCode.ServiceUnavailable, "temporarily_unavailable"));

        await _service.Awaiting(service => service.GetAccessTokenAsync(CurrentCancellationToken))
            .Should().ThrowAsync<InvalidOperationException>();

        _revoked.Should().BeFalse();
        _tokenEndpoint.Respond(Token("access-2", TimeSpan.FromHours(1)));
        (await _service.GetAccessTokenAsync(CurrentCancellationToken)).Should().Be("access-2");
    }

    [Fact]
    public async Task GetAccessTokenAsync_Should_RefreshOnce_When_CalledConcurrently()
    {
        await AuthenticateWithExpiringTokenAsync();
        _tokenEndpoint.Respond(Token("access-2", TimeSpan.FromHours(1)));

        var accessTokens = await Task.WhenAll(Enumerable.Range(0, 5)
            .Select(_ => _service.GetAccessTokenAsync(CurrentCancellationToken)));

        accessTokens.Should().AllBe("access-2");
        _tokenEndpoint.RequestCount.Should().Be(2);
    }

    private async Task AuthenticateWithExpiringTokenAsync()
    {
        _tokenEndpoint.Respond(Token("access-1", TimeSpan.FromSeconds(30)));
        await _service.AuthenticateAsync(CurrentCancellationToken);
    }

    private static HttpResponseMessage Token(string accessToken, TimeSpan expiresIn, string? refreshToken = null) =>
        new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new Dictionary<string, object?>
            {
                ["access_token"] = accessToken,
                ["token_type"] = "Bearer",
                ["expires_in"] = (int)expiresIn.TotalSeconds,
                ["refresh_token"] = refreshToken
            })
        };

    private static HttpResponseMessage Error(HttpStatusCode statusCode, string error) =>
        new(statusCode)
        {
            Content = JsonContent.Create(new Dictionary<string, string> { ["error"] = error })
        };

    private sealed class TokenEndpointHandler : HttpMessageHandler
    {
        private readonly ConcurrentQueue<HttpResponseMessage> _responses = new();
        private int _requestCount;

        public int RequestCount => _requestCount;

        public void Respond(HttpResponseMessage response) => _responses.Enqueue(response);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _requestCount);
            return Task.FromResult(_responses.TryDequeue(out var response)
                ? response
                : new HttpResponseMessage(HttpStatusCode.InternalServerError));
        }
    }
}