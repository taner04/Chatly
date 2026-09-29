using System.Security.Cryptography;
using System.Text;
using Chatly.WebApi.Common.Composition.Options;
using Chatly.WebApi.Features.Calls.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Chatly.WebApi.UnitTests.Tests.Services.Calls;

public sealed class LiveKitWebhookReceiverTests
{
    private const string Body =
        """{"event":"room_finished","room":{"sid":"RM_1","name":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"}}""";

    private static readonly LiveKitOption Option = new()
    {
        ServerUrl = "ws://livekit.test",
        ApiKey = "test-key",
        ApiSecret = "test-secret-with-at-least-thirty-two-chars"
    };

    private readonly LiveKitWebhookReceiver _receiver = new(Options.Create(Option));

    [Fact]
    public async Task ReceiveAsync_Should_ParseEvent_When_SignatureIsValid()
    {
        var webhookEvent = await _receiver.ReceiveAsync(Body, CreateToken(Body, Option.ApiSecret));

        webhookEvent.Should().Be(new LiveKitWebhookEvent(
            LiveKitWebhookEvent.RoomFinished,
            "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
    }

    [Fact]
    public async Task ReceiveAsync_Should_AcceptToken_When_BearerPrefixIsPresent()
    {
        var webhookEvent = await _receiver.ReceiveAsync(Body, "Bearer " + CreateToken(Body, Option.ApiSecret));

        webhookEvent.Should().NotBeNull();
    }

    [Fact]
    public async Task ReceiveAsync_Should_Reject_When_BodyWasTampered()
    {
        var token = CreateToken(Body, Option.ApiSecret);

        var webhookEvent = await _receiver.ReceiveAsync(Body.Replace("RM_1", "RM_2"), token);

        webhookEvent.Should().BeNull();
    }

    [Fact]
    public async Task ReceiveAsync_Should_Reject_When_SignedWithWrongSecret()
    {
        var webhookEvent = await _receiver.ReceiveAsync(
            Body,
            CreateToken(Body, "another-secret-with-at-least-thirty-two-chars"));

        webhookEvent.Should().BeNull();
    }

    [Fact]
    public async Task ReceiveAsync_Should_Reject_When_AuthorizationIsMissing() =>
        (await _receiver.ReceiveAsync(Body, null)).Should().BeNull();

    private static string CreateToken(string body, string secret) =>
        new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Option.ApiKey,
            Expires = DateTime.UtcNow.AddMinutes(5),
            Claims = new Dictionary<string, object>
            {
                ["sha256"] = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(body)))
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256)
        });
}