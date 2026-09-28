using System.Text.Json;
using Chatly.Contracts.Features.Hubs;
using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;

namespace Chatly.WebApi.UnitTests.Tests.Contracts.Calls;

public sealed class CallContractSerializationTests
{
    private static readonly Guid CallId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid RemoteUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly DateTimeOffset AcceptedAt = new(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);

    public static TheoryData<CallMessage> Notifications =>
    [
        new IncomingCallNotification(CallId, RemoteUserId, "remote-user", CallRole.Receiver, CallState.Ringing),
        new CallAcceptedNotification(CallId, RemoteUserId, "remote-user", CallRole.Caller, CallState.Active, AcceptedAt),
        new CallRejectedNotification(
            CallId,
            RemoteUserId,
            "remote-user",
            CallRole.Caller,
            CallState.Ended,
            CallEndReason.Declined),

        new CallEndedNotification(
            CallId,
            RemoteUserId,
            "remote-user",
            CallRole.Receiver,
            CallState.Ended,
            CallEndReason.Completed),

        new CallStateChangedNotification(CallId, RemoteUserId, "remote-user", CallRole.Receiver, CallState.Active, AcceptedAt)
    ];

    [Theory]
    [MemberData(nameof(Notifications))]
    public void Notification_Should_RoundTripPolymorphically_When_SerializedAsCall(CallMessage expected)
    {
        var json = JsonSerializer.Serialize<CallMessage>(expected);

        var actual = JsonSerializer.Deserialize<CallMessage>(json);

        json.Should().Contain("\"$callType\":");
        actual.Should().NotBeNull();
        actual.Should().BeOfType(expected.GetType());
        actual.Should().Be(expected);
        actual!.Role.Should().Be(expected.Role);
        actual.State.Should().Be(expected.State);
    }

    [Theory]
    [InlineData(CallRole.Caller)]
    [InlineData(CallRole.Receiver)]
    public void CallInfo_Should_KeepLocalRole_When_RoundTripped(CallRole role)
    {
        var expected = new CallInfo(
            CallId,
            RemoteUserId,
            "remote-user",
            role,
            CallState.Active,
            CallEndReason.Completed,
            AcceptedAt);

        var json = JsonSerializer.Serialize(expected);
        var actual = JsonSerializer.Deserialize<CallInfo>(json);

        actual.Should().Be(expected);
        actual!.Role.Should().Be(role);
    }

    [Fact]
    public void Every_Call_Type_Should_BeRegisteredForPolymorphism_When_ContractsChange()
    {
        var registered = typeof(CallMessage).Assembly.GetTypes()
            .Where(type => type is { IsAbstract: false } && type.IsSubclassOf(typeof(CallMessage)))
            .ToList();

        Notifications.Select(row => row.Data.GetType()).Should().BeEquivalentTo(registered);
    }
}
