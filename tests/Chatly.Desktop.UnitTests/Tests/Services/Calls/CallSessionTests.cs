using Chatly.Contracts.Features.Hubs;
using Chatly.Contracts.Features.Hubs.Abstraction;
using Chatly.Desktop.Models.Calls;
using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.Services.Calls;
using Chatly.Desktop.UnitTests.Infrastructure;
using Chatly.Desktop.UnitTests.Tests.Services.Calls.TestDoubles;

namespace Chatly.Desktop.UnitTests.Tests.Services.Calls;

public sealed class CallSessionTests : TestBase
{
    private static readonly DateTimeOffset AcceptedAt = new(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);

    private readonly Guid _callId = Guid.NewGuid();
    private readonly ICallingHubServer _hub = Substitute.For<ICallingHubServer>();
    private readonly FakeCallMediaHost _media = new();

    public CallSessionTests()
    {
        _hub.JoinMediaAsync(_callId).Returns(new CallMediaAccess("ws://media", "token"));
    }

    [Fact]
    public async Task RemoteParticipantEvents_Should_ConnectMediaAndThenEndCall_When_ParticipantJoinsAndLeaves()
    {
        var session = await CreateJoinedSessionAsync();

        session.Snapshot.IsMediaConnected.Should().BeFalse();
        _media.RaiseRemoteParticipantJoined();
        await FlushAsync(session);
        session.Snapshot.IsMediaConnected.Should().BeTrue();

        _media.RaiseRemoteParticipantLeft();
        await FlushAsync(session);

        await _hub.Received(1).EndCallAsync(_callId);
        session.Snapshot.Should().Be(CallSnapshot.Empty);
        _media.LeaveCount.Should().Be(1);
    }

    [Fact]
    public async Task MediaDisconnected_Should_EndCall_When_MediaIsJoined()
    {
        var session = await CreateJoinedSessionAsync();

        _media.RaiseDisconnected("SERVER_SHUTDOWN");
        await FlushAsync(session);

        await _hub.Received(1).EndCallAsync(_callId);
        session.Snapshot.Should().Be(CallSnapshot.Empty);
    }

    [Fact]
    public async Task SetMuted_Should_ToggleMicrophoneAndSnapshot_When_MediaIsJoined()
    {
        var session = await CreateJoinedSessionAsync();

        await session.RunAsync(() => session.SetMutedAsync(true), CurrentCancellationToken);
        session.Snapshot.IsMuted.Should().BeTrue();
        await session.RunAsync(() => session.SetMutedAsync(false), CurrentCancellationToken);

        session.Snapshot.IsMuted.Should().BeFalse();
        _media.MicrophoneStates.Should().Equal(false, true);
    }

    [Fact]
    public async Task ReconnectingChanged_Should_UpdateSnapshotAndKeepAcceptedAt_When_MediaReconnects()
    {
        var session = await CreateJoinedSessionAsync();

        _media.RaiseReconnectingChanged(true);
        await FlushAsync(session);
        session.Snapshot.IsReconnecting.Should().BeTrue();
        session.Snapshot.AcceptedAt.Should().Be(AcceptedAt);

        _media.RaiseReconnectingChanged(false);
        await FlushAsync(session);
        session.Snapshot.IsReconnecting.Should().BeFalse();
    }

    [Fact]
    public async Task CallSettings_Should_BeUsedOnJoinAndAppliedLive_When_SettingsChangeDuringCall()
    {
        var appSettings = CallSessionFactory.CreateAppSettings();
        appSettings.CallSettings.InputDeviceId = "mic-2";
        appSettings.CallSettings.NoiseSuppression = false;
        var session = await CreateJoinedSessionAsync(appSettings);

        appSettings.CallSettings.EchoCancellation = false;
        await FlushAsync(session);

        _media.AudioOptions.Should().Equal(
            new CallAudioOptions("mic-2", null, true, false, true),
            new CallAudioOptions("mic-2", null, false, false, true));
    }

    [Fact]
    public async Task MediaEvents_Should_BeIgnored_When_NoMediaIsJoined()
    {
        var session = CallSessionFactory.Create(_hub, _media, new FakeNotificationSoundPlayer());

        _media.RaiseDisconnected("SERVER_SHUTDOWN");
        _media.RaiseRemoteParticipantLeft();
        await FlushAsync(session);

        await _hub.DidNotReceive().EndCallAsync(Arg.Any<Guid>());
        _media.LeaveCount.Should().Be(0);
    }

    [Fact]
    public async Task EnsureMedia_Should_EndCallAndTearDown_When_JoiningMediaFails()
    {
        _hub.JoinMediaAsync(_callId)
            .Returns(Task.FromException<CallMediaAccess>(new InvalidOperationException("no media")));
        var session = CallSessionFactory.Create(_hub, _media, new FakeNotificationSoundPlayer());
        session.SetCall(new CallInfo(_callId, Guid.NewGuid(), "remote", CallRole.Caller, CallState.Active, null));

        var join = () => session.RunAsync(
            () => session.EnsureMediaAsync(CurrentCancellationToken),
            CurrentCancellationToken);

        await join.Should().ThrowAsync<InvalidOperationException>();
        await _hub.Received(1).EndCallAsync(_callId);
        session.Snapshot.Should().Be(CallSnapshot.Empty);
    }

    private async Task<CallSession> CreateJoinedSessionAsync(AppSettings? appSettings = null)
    {
        var session = CallSessionFactory.Create(_hub, _media, new FakeNotificationSoundPlayer(), appSettings);
        session.SetCall(new CallInfo(_callId, Guid.NewGuid(), "remote", CallRole.Caller, CallState.Active, null,
            AcceptedAt));
        await session.RunAsync(
            () => session.EnsureMediaAsync(CurrentCancellationToken),
            CurrentCancellationToken);
        return session;
    }

    private static Task FlushAsync(CallSession session) =>
        session.RunAsync(() => Task.CompletedTask, CurrentCancellationToken);
}