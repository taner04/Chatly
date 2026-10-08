using Chatly.Desktop.Abstraction.Authentication;
using Chatly.Desktop.Options;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Browser;
using Microsoft.Extensions.Options;

namespace Chatly.Desktop.Services.Authentication;

[SingletonService]
public sealed partial class AuthenticationService
{
    private static readonly TimeSpan AccessTokenRefreshMargin = TimeSpan.FromMinutes(1);

    private readonly OidcClient _client;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly SemaphoreSlim _operationLock = new(1, 1);
    private readonly OidcOption _options;
    private readonly ISecureTokenStore _secureTokenStore;
    private readonly UserSessionContext _sessionContext;
    private string? _accessToken;
    private DateTimeOffset _accessTokenExpiration;
    private string? _identityToken;

    public AuthenticationService(
        IOptions<OidcOption> oidcOptions,
        ISecureTokenStore secureTokenStore,
        UserSessionContext sessionContext,
        IBrowser browser,
        ILogger<AuthenticationService> logger)
        : this(
            oidcOptions.Value,
            new OidcClient(new OidcClientOptions
            {
                Authority = oidcOptions.Value.Authority,
                ClientId = oidcOptions.Value.ClientId,
                Scope = oidcOptions.Value.Scope,
                RedirectUri = oidcOptions.Value.RedirectUri,
                PostLogoutRedirectUri = oidcOptions.Value.RedirectUri,
                Browser = browser
            }),
            secureTokenStore,
            sessionContext,
            logger)
    {
    }

    internal AuthenticationService(
        OidcOption options,
        OidcClient client,
        ISecureTokenStore secureTokenStore,
        UserSessionContext sessionContext,
        ILogger<AuthenticationService> logger)
    {
        _options = options;
        _client = client;
        _secureTokenStore = secureTokenStore;
        _sessionContext = sessionContext;
        _logger = logger;
    }

    internal async Task<string> AuthenticateAsync(CancellationToken cancellationToken)
    {
        await _operationLock.WaitAsync(cancellationToken);
        try
        {
            return await AuthenticateCoreAsync(cancellationToken);
        }
        finally
        {
            _operationLock.Release();
        }
    }

    internal async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (IsAccessTokenValid())
        {
            return _accessToken;
        }

        var sessionEnded = false;
        await _operationLock.WaitAsync(cancellationToken);
        try
        {
            if (_accessToken is null || IsAccessTokenValid())
            {
                return _accessToken;
            }

            var refreshToken = await _secureTokenStore.TryReadRefreshTokenAsync(cancellationToken);
            var accessToken = refreshToken is null ? null : await TryRefreshAsync(refreshToken, cancellationToken);
            if (accessToken is null)
            {
                ClearTokens();
                sessionEnded = true;
            }

            return accessToken;
        }
        finally
        {
            _operationLock.Release();
            if (sessionEnded)
            {
                _sessionContext.NotifyDeviceSessionRevoked();
            }
        }
    }

    internal async Task LogoutAsync(CancellationToken cancellationToken)
    {
        await _operationLock.WaitAsync(cancellationToken);
        try
        {
            await LogoutCoreAsync(cancellationToken);
        }
        finally
        {
            _operationLock.Release();
        }
    }

    private async Task<string> AuthenticateCoreAsync(CancellationToken cancellationToken)
    {
        var refreshToken =
            await _secureTokenStore.TryReadRefreshTokenAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return await LoginAsync(cancellationToken);
        }

        var accessToken = await TryRefreshAsync(refreshToken, cancellationToken);
        if (accessToken is not null)
        {
            return accessToken;
        }

        await _secureTokenStore.DeleteRefreshTokenAsync(cancellationToken);

        return await LoginAsync(cancellationToken);
    }

    private async Task LogoutCoreAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _client.LogoutAsync(
                new LogoutRequest { IdTokenHint = _identityToken },
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            LogRemoteLogoutFailed(exception);
        }
        finally
        {
            ClearTokens();
            await _secureTokenStore.DeleteRefreshTokenAsync(CancellationToken.None);
        }
    }

    private bool IsAccessTokenValid() =>
        _accessToken is not null && DateTimeOffset.UtcNow < _accessTokenExpiration - AccessTokenRefreshMargin;

    private void SetAccessToken(string accessToken, DateTimeOffset expiration)
    {
        _accessToken = accessToken;
        _accessTokenExpiration = expiration;
    }

    private void ClearTokens()
    {
        _identityToken = null;
        _accessToken = null;
        _accessTokenExpiration = default;
    }

    private async Task<string> LoginAsync(CancellationToken cancellationToken)
    {
        var parameters = new Parameters();

        if (!string.IsNullOrWhiteSpace(_options.Prompt))
        {
            parameters.Add("prompt", _options.Prompt);
        }

        var result = await _client.LoginAsync(
            new LoginRequest
            {
                FrontChannelExtraParameters = parameters
            },
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        EnsureSuccessful(result);

        var accessToken = RequireToken(result.AccessToken, "access");
        var refreshToken = RequireToken(result.RefreshToken, "refresh");
        _identityToken = result.IdentityToken;
        SetAccessToken(accessToken, result.AccessTokenExpiration);

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

        if (result is { IsError: true, Error: "invalid_grant" })
        {
            return null;
        }

        EnsureSuccessful(result);

        var accessToken = RequireToken(result.AccessToken, "access");
        _identityToken = result.IdentityToken;
        SetAccessToken(accessToken, result.AccessTokenExpiration);
        var nextRefreshToken = string.IsNullOrWhiteSpace(result.RefreshToken)
            ? refreshToken
            : result.RefreshToken;

        await _secureTokenStore.SaveRefreshTokenAsync(nextRefreshToken, cancellationToken);
        return accessToken;
    }

    private static void EnsureSuccessful(Result result)
    {
        if (!result.IsError)
        {
            return;
        }

        var message = string.IsNullOrWhiteSpace(result.ErrorDescription)
            ? result.Error
            : $"{result.Error}: {result.ErrorDescription}";
        throw new InvalidOperationException(
            $"Authentication failed: {message ?? "Unknown error"}");
    }

    private static string RequireToken(string? token, string tokenType) =>
        !string.IsNullOrWhiteSpace(token)
            ? token
            : throw new InvalidOperationException($"The identity provider did not return a {tokenType} token.");

    [LoggerMessage(
        LogLevel.Warning,
        "Remote identity provider logout failed. Local credentials will still be removed.")]
    private partial void LogRemoteLogoutFailed(Exception exception);
}