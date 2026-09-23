using System.Text.Json;
using Chatly.Contracts.Features.Hubs;
using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;

namespace Chatly.Calls.Tests;

public sealed class CallContractSerializationTests
{
    private static readonly Guid CallId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid RemoteUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public static TheoryData<Call> Notifications =>
    [
        new IncomingCallNotification(CallId, RemoteUserId, "remote-user", CallRole.Receiver, CallState.Ringing),
        new CallAcceptedNotification(CallId, RemoteUserId, "remote-user", CallRole.Caller, CallState.Accepted),
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

        new CallStateChangedNotification(CallId, RemoteUserId, "remote-user", CallRole.Receiver, CallState.Active),
        new WebRtcOfferNotification(
            CallId,
            RemoteUserId,
            "remote-user",
            CallRole.Receiver,
            CallState.Offered,
            "offer-sdp"),

        new WebRtcAnswerNotification(
            CallId,
            RemoteUserId,
            "remote-user",
            CallRole.Caller,
            CallState.Active,
            "answer-sdp"),

        new IceCandidateNotification(
            CallId,
            RemoteUserId,
            "remote-user",
            CallRole.Caller,
            CallState.Active,
            "candidate-payload",
            "audio",
            7)
    ];

    [Theory]
    [MemberData(nameof(Notifications))]
    public void Notification_round_trips_polymorphically_through_call(Call expected)
    {
        var json = JsonSerializer.Serialize<Call>(expected);

        var actual = JsonSerializer.Deserialize<Call>(json);

        Assert.Contains("\"$callType\":", json);
        Assert.NotNull(actual);
        Assert.IsType(expected.GetType(), actual);
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Role, actual.Role);
        Assert.Equal(expected.State, actual.State);
    }

    [Theory]
    [InlineData(CallRole.Caller)]
    [InlineData(CallRole.Receiver)]
    public void CallInfo_round_trip_restores_local_role(CallRole role)
    {
        var expected = new CallInfo(
            CallId,
            RemoteUserId,
            "remote-user",
            role,
            CallState.Active,
            CallEndReason.Completed);

        var json = JsonSerializer.Serialize(expected);
        var actual = JsonSerializer.Deserialize<CallInfo>(json);

        Assert.Equal(expected, actual);
        Assert.Equal(role, actual!.Role);
    }
}
