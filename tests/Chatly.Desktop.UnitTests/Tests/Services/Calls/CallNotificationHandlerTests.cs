using Chatly.Contracts.Features.Hubs;
using Chatly.Contracts.Features.Hubs.Abstraction;
using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;
using Chatly.Desktop.Services.Api.Hubs.CallHub.CallHandlers;
using Chatly.Desktop.Services.Calls;
using Chatly.Desktop.UnitTests.Infrastructure;
using Chatly.Desktop.UnitTests.Tests.Services.Calls.TestDoubles;

namespace Chatly.Desktop.UnitTests.Tests.Services.Calls;

public sealed class CallNotificationHandlerTests : TestBase
{
    private static readonly DateTimeOffset AcceptedAt = new(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);

    private readonly Guid _callId = Guid.NewGuid();
    private readonly ICallingHubServer _hub = Substitute.For<ICallingHubServer>();
    private readonly FakeCallMediaHost _media = new();
    private readonly Guid _remoteUserId = Guid.NewGuid();
    private readonly CallSession _session;
    private readonly FakeNotificationSoundPlayer _soundPlayer = new();

    public CallNotificationHandlerTests()
    {
        _hub.JoinMediaAsync(Arg.Any<Guid>()).Returns(new CallMediaAccess("ws://media", "token"));
        _session = CallSessionFactory.Create(_hub, _media, _soundPlayer);
    }

    [Fact]
    public async Task IncomingCall_Should_SetCallAndRing_When_NoCallIsActive()
    {
        await new IncomingCallNotificationHandler(_session).HandleAsync(Incoming(_callId));

        _session.Snapshot.CallId.Should().Be(_callId);
        _session.Snapshot.IsIncoming.Should().BeTrue();
        _soundPlayer.Playbacks.Should().ContainSingle();
    }

    [Fact]
    public async Task IncomingCall_Should_BeIgnored_When_AnotherCallIsActive()
    {
        await new IncomingCallNotificationHandler(_session).HandleAsync(Incoming(_callId));

        await new IncomingCallNotificationHandler(_session).HandleAsync(Incoming(Guid.NewGuid()));

        _session.Snapshot.CallId.Should().Be(_callId);
        _soundPlayer.Playbacks.Should().ContainSingle();
    }

    [Fact]
    public async Task CallEnded_Should_TearDownAndStopRinging_When_CurrentCallEnds()
    {
        await new IncomingCallNotificationHandler(_session).HandleAsync(Incoming(_callId));

        await new CallEndedNotificationHandler(_session).HandleAsync(Ended(_callId));

        _session.Snapshot.Should().Be(CallSnapshot.Empty);
        _soundPlayer.Playbacks[0].StopCount.Should().Be(1);
        _soundPlayer.Playbacks[0].DisposeCount.Should().Be(1);
    }

    [Fact]
    public async Task CallEnded_Should_BeIgnored_When_ItBelongsToAnotherCall()
    {
        await new IncomingCallNotificationHandler(_session).HandleAsync(Incoming(_callId));

        await new CallEndedNotificationHandler(_session).HandleAsync(Ended(Guid.NewGuid()));

        _session.Snapshot.CallId.Should().Be(_callId);
        _soundPlayer.Playbacks[0].StopCount.Should().Be(0);
    }

    [Fact]
    public async Task CallRejected_Should_TearDownAndStopRinging_When_ReceiverDeclines()
    {
        _session.SetCall(new CallInfo(_callId, _remoteUserId, "receiver", CallRole.Caller, CallState.Ringing, null));
        await _session.RunAsync(() => _session.StartOutgoingToneAsync(), CurrentCancellationToken);

        await new CallRejectedNotificationHandler(_session).HandleAsync(new CallRejectedNotification(
            _callId,
            _remoteUserId,
            "receiver",
            CallRole.Caller,
            CallState.Ended,
            CallEndReason.Declined));

        _session.Snapshot.Should().Be(CallSnapshot.Empty);
        _soundPlayer.Playbacks.Should().ContainSingle().Which.StopCount.Should().Be(1);
    }

    [Fact]
    public async Task CallAccepted_Should_StopRingingAndJoinMedia_When_CallerIsNotified()
    {
        _session.SetCall(new CallInfo(_callId, _remoteUserId, "receiver", CallRole.Caller, CallState.Ringing, null));
        await _session.RunAsync(() => _session.StartOutgoingToneAsync(), CurrentCancellationToken);

        await new CallAcceptedNotificationHandler(_session).HandleAsync(Accepted(_callId));

        _media.Joins.Should().ContainSingle().Which.Should().Be(("ws://media", "token"));
        _session.Snapshot.State.Should().Be(CallState.Active);
        _session.Snapshot.AcceptedAt.Should().Be(AcceptedAt);
        _soundPlayer.Playbacks.Should().ContainSingle().Which.StopCount.Should().Be(1);
    }

    [Fact]
    public async Task CallAccepted_Should_NotJoinMedia_When_CallIsOnAnotherDevice()
    {
        await new CallStateChangedNotificationHandler(_session).HandleAsync(StateChanged(_callId));

        await new CallAcceptedNotificationHandler(_session).HandleAsync(Accepted(_callId));

        _media.Joins.Should().BeEmpty();
        _session.Snapshot.IsOnAnotherDevice.Should().BeTrue();
    }

    [Fact]
    public async Task CallStateChanged_Should_MarkCallOnAnotherDeviceAndStopRinging_When_OtherDeviceAccepts()
    {
        await new IncomingCallNotificationHandler(_session).HandleAsync(Incoming(_callId));

        await new CallStateChangedNotificationHandler(_session).HandleAsync(StateChanged(_callId));

        _session.Snapshot.IsOnAnotherDevice.Should().BeTrue();
        _session.Snapshot.State.Should().Be(CallState.Active);
        _session.Snapshot.AcceptedAt.Should().Be(AcceptedAt);
        _soundPlayer.Playbacks.Should().ContainSingle().Which.StopCount.Should().Be(1);
        _media.Joins.Should().BeEmpty();
    }

    [Fact]
    public async Task CallStateChanged_Should_BeIgnored_When_ItBelongsToAnotherCall()
    {
        await new IncomingCallNotificationHandler(_session).HandleAsync(Incoming(_callId));

        await new CallStateChangedNotificationHandler(_session).HandleAsync(StateChanged(Guid.NewGuid()));

        _session.Snapshot.CallId.Should().Be(_callId);
        _session.Snapshot.IsOnAnotherDevice.Should().BeFalse();
    }

    private IncomingCallNotification Incoming(Guid callId) =>
        new(callId, _remoteUserId, "caller", CallRole.Receiver, CallState.Ringing);

    private CallEndedNotification Ended(Guid callId) =>
        new(callId, _remoteUserId, "caller", CallRole.Receiver, CallState.Ended, CallEndReason.Completed);

    private CallAcceptedNotification Accepted(Guid callId) =>
        new(callId, _remoteUserId, "receiver", CallRole.Caller, CallState.Active, AcceptedAt);

    private CallStateChangedNotification StateChanged(Guid callId) =>
        new(callId, _remoteUserId, "caller", CallRole.Receiver, CallState.Active, AcceptedAt);
}