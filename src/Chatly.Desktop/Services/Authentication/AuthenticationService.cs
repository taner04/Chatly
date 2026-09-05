using Chatly.Desktop.Abstraction.Authentication;
using Chatly.Desktop.Options;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Browser;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chatly.Desktop.Services.Authentication;

[SingletonService]
public sealed partial class AuthenticationService
{
    private readonly OidcClient _client;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly SemaphoreSlim _operationLock = new(1, 1);
    private readonly Auth0Option _options;
    private readonly ISecureTokenStore _secureTokenStore;
    private string? _identityToken;

    public AuthenticationService(
        IOptions<Auth0Option> auth0Options,
        ISecureTokenStore secureTokenStore,
        IBrowser browser,
        ILogger<AuthenticationService> logger)
    {
        _options = auth0Options.Value;
        _secureTokenStore = secureTokenStore;
        _logger = logger;
        _client = new OidcClient(new OidcClientOptions
        {
            Authority = $"https://{_options.Domain}",
            ClientId = _options.ClientId,
            Scope = _options.Scope,
            RedirectUri = _options.RedirectUri,
            PostLogoutRedirectUri = _options.RedirectUri,
            Browser = browser
        });
    }

    public async Task<string> AuthenticateAsync(CancellationToken cancellationToken)
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

    public async Task LogoutAsync(CancellationToken cancellationToken)
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
            _identityToken = null;
            await _secureTokenStore.DeleteRefreshTokenAsync(CancellationToken.None);
        }
    }

    private async Task<string> LoginAsync(CancellationToken cancellationToken)
    {
        var parameters = new Parameters
        {
            { "connection", _options.ConnectionName },
            { "audience", _options.Audience }
        };

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

        EnsureSuccessful(result);

        var accessToken = RequireToken(result.AccessToken, "access");
        _identityToken = result.IdentityToken;
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

    private static string RequireToken(string? token, string tokenType)
    {
        return !string.IsNullOrWhiteSpace(token)
            ? token
            : throw new InvalidOperationException($"Auth0 did not return a {tokenType} token.");
    }

    [LoggerMessage(
        LogLevel.Warning,
        "Remote Auth0 logout failed. Local credentials will still be removed.")]
    private partial void LogRemoteLogoutFailed(Exception exception);
}