using System;
using System.Threading;
using System.Threading.Tasks;
using Chatly.Desktop.Abstractions;
using Chatly.Desktop.Options;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using Microsoft.Extensions.Options;

namespace Chatly.Desktop.Infrastructure.Auth;

public sealed class AuthenticationService
{
    private readonly OidcClient _client;
    private readonly UserContext _userContext;
    private readonly ISecureTokenStore _secureTokenStore;
    private readonly Auth0Option _options;

    public AuthenticationService(
        IOptions<Auth0Option> auth0Options,
        UserContext userContext,
        ISecureTokenStore secureTokenStore)
    {
        _options = auth0Options.Value;
        _userContext = userContext;
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

    public async Task RestoreOrLoginAsync(
        CancellationToken cancellationToken)
    {
        var refreshToken =
            await _secureTokenStore.TryReadRefreshTokenAsync(
                cancellationToken);

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var refreshed = await TryRefreshAsync(
                refreshToken,
                cancellationToken);

            if (refreshed)
            {
                return;
            }

            await _secureTokenStore.DeleteRefreshTokenAsync(
                cancellationToken);
        }

        await LoginAsync(cancellationToken);
    }

    private async Task LoginAsync(
        CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequest
        {
            FrontChannelExtraParameters = new Parameters
            {
                { "connection", _options.ConnectionName },
                { "audience", _options.Audience }
            }
        };

        var result = await _client.LoginAsync(
            loginRequest,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsError)
        {
            throw CreateAuthenticationException(
                result.Error,
                result.ErrorDescription);
        }

        if (string.IsNullOrWhiteSpace(result.AccessToken))
        {
            throw new InvalidOperationException(
                "Auth0 did not return an access token.");
        }

        _userContext.AccessToken = result.AccessToken;

        if (!string.IsNullOrWhiteSpace(result.RefreshToken))
        {
            await _secureTokenStore.SaveRefreshTokenAsync(
                result.RefreshToken,
                cancellationToken);
        }
    }

    private async Task<bool> TryRefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var result = await _client.RefreshTokenAsync(
            refreshToken,
            cancellationToken: cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsError)
        {
            // The saved refresh token has expired, been revoked, or was invalidated by refresh-token rotation.
            if (string.Equals(
                    result.Error,
                    "invalid_grant",
                    StringComparison.Ordinal))
            {
                return false;
            }

            // Not delete the token for temporary network/server errors.
            throw CreateAuthenticationException(
                result.Error,
                result.ErrorDescription);
        }

        if (string.IsNullOrWhiteSpace(result.AccessToken))
        {
            throw new InvalidOperationException(
                "Auth0 did not return an access token during refresh.");
        }
        
        var nextRefreshToken =
            string.IsNullOrWhiteSpace(result.RefreshToken)
                ? refreshToken
                : result.RefreshToken;

        await _secureTokenStore.SaveRefreshTokenAsync(
            nextRefreshToken,
            cancellationToken);

        var userInfo = await _client.GetUserInfoAsync(
            result.AccessToken,
            cancellationToken);

        if (userInfo.IsError)
        {
            throw CreateAuthenticationException(
                userInfo.Error,
                userInfo.ErrorDescription);
        }

        _userContext.AccessToken = result.AccessToken;
        return true;
    }

    public async Task LogoutAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await _client.LogoutAsync(
                cancellationToken: cancellationToken);
        }
        finally
        {
            // Local logout must succeed even if the provider request fails.
            await _secureTokenStore.DeleteRefreshTokenAsync(CancellationToken.None);

            _userContext.AccessToken = null!;
        }
    }

    private static InvalidOperationException CreateAuthenticationException(
        string? error,
        string? description)
    {
        var message = string.IsNullOrWhiteSpace(description)
            ? error
            : $"{error}: {description}";

        return new InvalidOperationException(
            $"Authentication failed: {message ?? "Unknown error"}");
    }
}