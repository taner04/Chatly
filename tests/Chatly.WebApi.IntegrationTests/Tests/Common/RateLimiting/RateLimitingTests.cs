using System.Text;
using System.Text.Json;
using Chatly.WebApi.Common.Composition.Options;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace Chatly.WebApi.IntegrationTests.Tests.Common.RateLimiting;

public sealed class RateLimitingTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task Request_Should_Return429WithRetryAfter_When_UserExceedsHttpLimit()
    {
        var client = CreateAuthenticatedClient();
        for (var i = 0; i < GetOption().HttpPermitLimit; i++)
        {
            var allowed = await client.GetCurrentUserAsync(CurrentCancellationToken);
            allowed.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        var response = await client.GetCurrentUserAsync(CurrentCancellationToken);

        AssertError(response, HttpStatusCode.TooManyRequests, "RateLimit.Exceeded");
        response.Headers.Should().ContainKey("Retry-After");
    }

    [Fact]
    public async Task Request_Should_Succeed_When_AnotherUserExceededHttpLimit()
    {
        var other = await CreateUserAsync("other");
        var otherClient = CreateAuthenticatedClient(other);
        for (var i = 0; i <= GetOption().HttpPermitLimit; i++)
        {
            _ = await otherClient.GetCurrentUserAsync(CurrentCancellationToken);
        }

        var response = await CreateAuthenticatedClient().GetCurrentUserAsync(CurrentCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LiveKitWebhook_Should_NotBeRateLimited()
    {
        var client = CreateHttpClient();
        var body = JsonSerializer.Serialize(new { @event = "room_finished" });

        for (var i = 0; i <= GetOption().HttpPermitLimit; i++)
        {
            var response = await client.PostAsync(
                ApiRoutes.LiveKit.Webhook,
                new StringContent(body, Encoding.UTF8, "application/webhook+json"),
                CurrentCancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task HubInvocation_Should_Fail_When_UserExceedsHubLimit()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        await using var hub = await ConnectNotificationHubAsync();
        for (var i = 0; i < GetOption().HubPermitLimit; i++)
        {
            await hub.StartTypingAsync(chatId.Value);
        }

        var rejected = await hub.Awaiting(client => client.StartTypingAsync(chatId.Value))
            .Should().ThrowExactlyAsync<HubException>();

        rejected.Which.Message.Should().Contain("Too many requests");
    }

    [Fact]
    public async Task HubConnect_Should_CloseConnection_When_UserExceedsConnectionLimit()
    {
        for (var i = 0; i < GetOption().HubConnectionPermitLimit; i++)
        {
            await using var allowed = await ConnectNotificationHubAsync();
            allowed.Connection.State.Should().Be(HubConnectionState.Connected);
        }

        await using var rejected = await ConnectNotificationHubAsync();

        var closed = await WaitForAsync(() => Task.FromResult<object?>(
            rejected.Connection.State == HubConnectionState.Disconnected ? true : null));
        closed.Should().NotBeNull();
    }

    private RateLimitingOption GetOption()
    {
        using var scope = CreateScope();
        return scope.ServiceProvider.GetRequiredService<IOptions<RateLimitingOption>>().Value;
    }
}
