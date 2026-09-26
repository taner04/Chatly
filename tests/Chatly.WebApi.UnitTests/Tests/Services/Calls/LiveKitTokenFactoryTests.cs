using System.Text;
using System.Text.Json;
using Chatly.WebApi.Common.Composition.Options;
using Chatly.WebApi.Features.Calls.Models;
using Chatly.WebApi.Features.Calls.Services;
using Chatly.WebApi.Features.Users.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Chatly.WebApi.UnitTests.Tests.Services.Calls;

public sealed class LiveKitTokenFactoryTests
{
    private static readonly LiveKitOption Option = new()
    {
        ServerUrl = "ws://127.0.0.1:7880",
        ApiKey = "test-key",
        ApiSecret = "test-secret-with-at-least-thirty-two-chars"
    };

    [Fact]
    public async Task CreateJoinToken_Should_SignForApiKeyAndGrantOnlyMicrophoneInCallRoom_When_UserJoins()
    {
        var factory = new LiveKitTokenFactory(Options.Create(Option));
        var callId = CallId.From(Guid.NewGuid());
        var userId = UserId.From(Guid.NewGuid());

        var token = factory.CreateJoinToken(callId, userId, "alice");

        var validation = await new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = Option.ApiKey,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Option.ApiSecret))
        });
        validation.IsValid.Should().BeTrue(validation.Exception?.Message);

        var jwt = new JsonWebToken(token);
        jwt.Subject.Should().Be(userId.Value.ToString("D"));
        jwt.GetClaim("name").Value.Should().Be("alice");
        (jwt.ValidTo - jwt.ValidFrom).Should().Be(LiveKitTokenFactory.TokenLifetime);

        using var video = JsonDocument.Parse(jwt.GetPayloadValue<JsonElement>("video").GetRawText());
        var grant = video.RootElement;
        grant.GetProperty("room").GetString().Should().Be(callId.Value.ToString("D"));
        grant.GetProperty("roomJoin").GetBoolean().Should().BeTrue();
        grant.GetProperty("canPublish").GetBoolean().Should().BeTrue();
        grant.GetProperty("canSubscribe").GetBoolean().Should().BeTrue();
        grant.GetProperty("canPublishData").GetBoolean().Should().BeFalse();
        grant.GetProperty("canPublishSources").EnumerateArray().Select(source => source.GetString())
            .Should().Equal("microphone");
    }

    [Fact]
    public void ServerUrl_Should_ComeFromOptions_When_FactoryIsCreated()
    {
        new LiveKitTokenFactory(Options.Create(Option)).ServerUrl.Should().Be(Option.ServerUrl);
    }
}
