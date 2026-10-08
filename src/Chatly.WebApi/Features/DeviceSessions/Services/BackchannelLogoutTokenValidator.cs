using System.Text.Json;
using Chatly.WebApi.Common.Composition.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Chatly.WebApi.Features.DeviceSessions.Services;

[ScopedService]
internal sealed class BackchannelLogoutTokenValidator(
    IOptionsMonitor<JwtBearerOptions> jwtBearerOptions,
    IOptions<OidcOption> oidcOptions)
{
    private const string BackchannelLogoutEvent = "http://schemas.openid.net/event/backchannel-logout";

    internal async Task<string?> ValidateAsync(string? logoutToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(logoutToken))
        {
            return null;
        }

        var bearerOptions = jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme);
        var configuration = bearerOptions.Configuration
                            ?? await bearerOptions.ConfigurationManager!.GetConfigurationAsync(cancellationToken);
        var oidcOption = oidcOptions.Value;

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(logoutToken, new TokenValidationParameters
        {
            ValidIssuer = oidcOption.Authority,
            ValidAudience = oidcOption.BackchannelLogoutAudience,
            IssuerSigningKeys = configuration.SigningKeys
        });

        return result is { IsValid: true, SecurityToken: JsonWebToken token } && IsLogoutToken(token)
                                                                              && token.TryGetPayloadValue<string>(
                                                                                  CurrentUserService.SessionIdClaim,
                                                                                  out var sessionId)
            ? sessionId
            : null;
    }

    private static bool IsLogoutToken(JsonWebToken token)
    {
        if (token.TryGetPayloadValue<string>("nonce", out _)
            || token.Claims.FirstOrDefault(claim => claim.Type == "events")?.Value is not { } events)
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(events);
            return document.RootElement.TryGetProperty(BackchannelLogoutEvent, out _);
        }
        catch (JsonException)
        {
            return false;
        }
    }
}