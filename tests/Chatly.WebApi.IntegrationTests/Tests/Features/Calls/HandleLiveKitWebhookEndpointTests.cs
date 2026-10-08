using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Chatly.Contracts.Features.Hubs;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Calls;

public sealed class HandleLiveKitWebhookEndpointTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task Webhook_Should_Return401_When_SignatureIsMissing()
    {
        var response = await CreateHttpClient().PostAsync(
            ApiRoutes.LiveKit.Webhook,
            new StringContent(RoomFinished(Guid.NewGuid()), Encoding.UTF8, "application/webhook+json"),
            CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Webhook_Should_EndActiveCallAsFailed_When_RoomFinished()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        await using var receiver = await ConnectCallHubAsync(friend);
        var started = await caller.StartCallAsync(friend.Id.Value);
        await receiver.AcceptCallAsync(started.CallId);
        var body = RoomFinished(started.CallId);
        using var request = new HttpRequestMessage(HttpMethod.Post, ApiRoutes.LiveKit.Webhook);
        request.Content = new StringContent(body, Encoding.UTF8, "application/webhook+json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateSignature(body));

        var response = await CreateHttpClient().SendAsync(request, CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var dbContext = GetDbContext();
        var call = await dbContext.Calls.SingleAsync(CurrentCancellationToken);
        call.Status.Should().Be(CallState.Ended);
        call.EndReason.Should().Be(CallEndReason.Failed);
    }

    private static string RoomFinished(Guid callId) =>
        JsonSerializer.Serialize(new
        {
            @event = "room_finished",
            room = new { sid = "RM_test", name = callId.ToString("D") }
        });

    private static string CreateSignature(string body) =>
        new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = TestSettings.LiveKitApiKey,
            Expires = DateTime.UtcNow.AddMinutes(5),
            Claims = new Dictionary<string, object>
            {
                ["sha256"] = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(body)))
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSettings.LiveKitApiSecret)),
                SecurityAlgorithms.HmacSha256)
        });
}