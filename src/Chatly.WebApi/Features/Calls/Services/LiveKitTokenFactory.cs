using System.Text;
using Chatly.WebApi.Common.Composition.Options;
using Chatly.WebApi.Features.Calls.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Chatly.WebApi.Features.Calls.Services;

[SingletonService]
internal sealed class LiveKitTokenFactory(IOptions<LiveKitOption> options)
{
    internal static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);

    private readonly JsonWebTokenHandler _handler = new();

    internal string ServerUrl => options.Value.ServerUrl;

    internal static string RoomName(CallId callId) => callId.Value.ToString("D");

    internal string CreateJoinToken(CallId callId, UserId userId, string? displayName)
    {
        var option = options.Value;
        var now = DateTime.UtcNow;
        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = userId.Value.ToString("D"),
            ["video"] = new Dictionary<string, object>
            {
                ["room"] = RoomName(callId),
                ["roomJoin"] = true,
                ["canPublish"] = true,
                ["canSubscribe"] = true,
                ["canPublishData"] = false,
                ["canPublishSources"] = new[] { "microphone" }
            }
        };

        if (!string.IsNullOrWhiteSpace(displayName))
        {
            claims["name"] = displayName;
        }

        return _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = option.ApiKey,
            NotBefore = now,
            IssuedAt = now,
            Expires = now + TokenLifetime,
            Claims = claims,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(option.ApiSecret)),
                SecurityAlgorithms.HmacSha256)
        });
    }
}