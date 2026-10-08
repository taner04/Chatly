using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Chatly.WebApi.Common.Composition.Options;
using Microsoft.Extensions.Options;

namespace Chatly.WebApi.Features.DeviceSessions.Services;

internal sealed partial class IdentitySessionClient(
    HttpClient httpClient,
    IOptions<OidcOption> oidcOptions,
    IOptions<IdentityAdminOption> adminOptions,
    ILogger<IdentitySessionClient> logger)
{
    private readonly IdentityAdminOption _adminOption = adminOptions.Value;
    private readonly OidcOption _oidcOption = oidcOptions.Value;

    internal async Task RevokeAsync(IEnumerable<string?> identitySessionIds, CancellationToken cancellationToken)
    {
        var sessionIds = identitySessionIds.OfType<string>().Distinct().ToList();
        if (sessionIds.Count == 0)
        {
            return;
        }

        try
        {
            var accessToken = await GetAccessTokenAsync(cancellationToken);
            foreach (var sessionId in sessionIds)
            {
                await RevokeSessionAsync(sessionId, accessToken, cancellationToken);
            }
        }
        catch (Exception exception) when (
            exception is HttpRequestException or JsonException or TaskCanceledException
            && !cancellationToken.IsCancellationRequested)
        {
            LogRevocationFailed(exception);
        }
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _adminOption.ClientId,
            ["client_secret"] = _adminOption.ClientSecret
        });
        using var response = await httpClient.PostAsync(_oidcOption.TokenEndpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
        return token?.AccessToken
               ?? throw new HttpRequestException("The identity provider did not return an access token.");
    }

    private async Task RevokeSessionAsync(string sessionId, string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"{_oidcOption.SessionsAdminEndpoint}/{Uri.EscapeDataString(sessionId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response is { IsSuccessStatusCode: false, StatusCode: not HttpStatusCode.NotFound })
        {
            LogSessionRevocationRejected(sessionId, (int)response.StatusCode);
        }
    }

    [LoggerMessage(LogLevel.Warning, "Identity provider sessions could not be revoked.")]
    private partial void LogRevocationFailed(Exception exception);

    [LoggerMessage(LogLevel.Warning,
        "Identity provider rejected revoking session {SessionId} with status {StatusCode}.")]
    private partial void LogSessionRevocationRejected(string sessionId, int statusCode);

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")]
        string? AccessToken);
}