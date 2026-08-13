using System;
using System.Threading;
using System.Threading.Tasks;
using Chatly.Desktop.Common.Options;
using Chatly.Desktop.Features.Authentication.Abstractions;
using Chatly.Desktop.Infrastructure.Authentication;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using Microsoft.Extensions.Options;

namespace Chatly.Desktop.Features.Authentication.Services;

public sealed class AuthenticationService
{
    private readonly OidcClient _client;
    private readonly Auth0Option _options;
    private readonly ISecureTokenStore _secureTokenStore;

    public AuthenticationService(
        IOptions<Auth0Option> auth0Options,
        ISecureTokenStore secureTokenStore)
    {
        _options = auth0Options.Value;
        _secureTokenStore = secureTokenStore;
        _client = new OidcClient(new OidcClientOptions
        {
            Authority = $"https://{_options.Domain}",
            ClientId = _options.ClientId,
            Scope = _options.Scope,
            RedirectUri = _options.RedirectUri,
            Browser = new AvaloniaAuthenticationBrowser()
        });
    }

    public async Task<string> AuthenticateAsync(CancellationToken cancellationToken)
    {
        var refreshToken =
            await _secureTokenStore.TryReadRefreshTokenAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var accessToken = await TryRefreshAsync(refreshToken, cancellationToken);
            if (accessToken is not null)
            {
                return accessToken;
            }

            await _secureTokenStore.DeleteRefreshTokenAsync(cancellationToken);
        }

        return await LoginAsync(cancellationToken);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _client.LogoutAsync(cancellationToken: cancellationToken);
        }
        finally
        {
            await _secureTokenStore.DeleteRefreshTokenAsync(CancellationToken.None);
        }
    }

    private async Task<string> LoginAsync(CancellationToken cancellationToken)
    {
        var result = await _client.LoginAsync(
            new LoginRequest
            {
                FrontChannelExtraParameters = new Parameters
                {
                    { "connection", _options.ConnectionName },
                    { "audience", _options.Audience }
                }
            },
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        EnsureSuccessful(result.IsError, result.Error, result.ErrorDescription);

        var accessToken = RequireToken(result.AccessToken, "access");
        var refreshToken = RequireToken(result.RefreshToken, "refresh");

        await _secureTokenStore.SaveRefreshTokenAsync(refreshToken, cancellationToken);
        return accessToken;
    }

    private async Task<string?> TryRefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var result = await _client.RefreshTokenAsync(
            refreshToken,
            cancellationToken: cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsError && string.Equals(result.Error, "invalid_grant", StringComparison.Ordinal))
        {
            return null;
        }

        EnsureSuccessful(result.IsError, result.Error, result.ErrorDescription);

        var accessToken = RequireToken(result.AccessToken, "access");
        var nextRefreshToken = string.IsNullOrWhiteSpace(result.RefreshToken)
            ? refreshToken
            : result.RefreshToken;

        await _secureTokenStore.SaveRefreshTokenAsync(nextRefreshToken, cancellationToken);
        return accessToken;
    }

    private static void EnsureSuccessful(bool isError, string? error, string? description)
    {
        if (!isError)
        {
            return;
        }

        var message = string.IsNullOrWhiteSpace(description) ? error : $"{error}: {description}";
        throw new InvalidOperationException(
            $"Authentication failed: {message ?? "Unknown error"}");
    }

    private static string RequireToken(string? token, string tokenType) =>
        !string.IsNullOrWhiteSpace(token)
            ? token
            : throw new InvalidOperationException($"Auth0 did not return a {tokenType} token.");
}
