using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Chatly.WebApi.Common.Composition.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Chatly.WebApi.Features.Calls.Services;

[SingletonService]
internal sealed class LiveKitWebhookReceiver(IOptions<LiveKitOption> options)
{
    private const string BearerPrefix = "Bearer ";
    private const string BodyHashClaim = "sha256";

    private readonly JsonWebTokenHandler _handler = new();

    internal async Task<LiveKitWebhookEvent?> ReceiveAsync(string body, string? authorization)
    {
        if (string.IsNullOrWhiteSpace(authorization))
        {
            return null;
        }

        var token = authorization.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? authorization[BearerPrefix.Length..]
            : authorization;

        var option = options.Value;
        var validation = await _handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = option.ApiKey,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(option.ApiSecret))
        });

        if (!validation.IsValid
            || !validation.Claims.TryGetValue(BodyHashClaim, out var bodyHash)
            || bodyHash is not string expectedHash
            || !CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(expectedHash),
                Encoding.ASCII.GetBytes(Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(body))))))
        {
            return null;
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var eventName = root.TryGetProperty("event", out var eventElement) ? eventElement.GetString() : null;
        var roomName = root.TryGetProperty("room", out var room) && room.TryGetProperty("name", out var name)
            ? name.GetString()
            : null;

        return new LiveKitWebhookEvent(eventName ?? string.Empty, roomName);
    }
}

internal sealed record LiveKitWebhookEvent(string Name, string? RoomName)
{
    internal const string RoomFinished = "room_finished";
}
