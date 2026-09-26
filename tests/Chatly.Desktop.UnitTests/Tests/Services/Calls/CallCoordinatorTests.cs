using Chatly.Contracts.Features.Hubs;
using Chatly.Contracts.Features.Hubs.Abstraction;
using Chatly.Desktop.Services.Calls;
using Chatly.Desktop.UnitTests.Tests.Services.Calls.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chatly.Desktop.UnitTests.Tests.Services.Calls;

public sealed class CallCoordinatorTests
{
    private readonly Guid _callId = Guid.NewGuid();
    private readonly CallCoordinator _coordinator;
    private readonly ICallingHubServer _hub = Substitute.For<ICallingHubServer>();
    private readonly FakeCallMediaHost _media = new();
    private readonly CallSession _session;
    private readonly FakeNotificationSoundPlayer _soundPlayer = new();

    public CallCoordinatorTests()
    {
        _hub.JoinMediaAsync(Arg.Any<Guid>()).Returns(new CallMediaAccess("ws://media", "token"));
        _session = CallSessionFactory.Create(_hub, _media, _soundPlayer);
        _coordinator = new CallCoordinator(_hub, _session, NullLogger<CallCoordinator>.Instance);
    }

    [Fact]
    public async Task EndAsync_Should_TearDownLocalMedia_When_HubCallFails()
    {
        _hub.EndCallAsync(_callId).Returns(Task.FromException(new InvalidOperationException("The hub is not connected.")));
        await JoinActiveCallAsync(_callId);

        var end = () => _coordinator.EndAsync(TestContext.Current.CancellationToken);

        await end.Should().ThrowAsync<InvalidOperationException>();
        _session.Snapshot.Should().Be(CallSnapshot.Empty);
        _media.LeaveCount.Should().Be(1);
    }

    [Fact]
    public async Task AcceptAsync_Should_ApplyServerCallInfoAndJoinMedia_When_CallIsIncoming()
    {
        var acceptedAt = new DateTimeOffset(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);
        _hub.AcceptCallAsync(_callId).Returns(new CallInfo(
            _callId,
            Guid.NewGuid(),
            "caller",
            CallRole.Receiver,
            CallState.Active,
            null,
            acceptedAt));
        _session.SetCall(new CallInfo(_callId, Guid.NewGuid(), "caller", CallRole.Receiver, CallState.Ringing, null));

        await _coordinator.AcceptAsync(TestContext.Current.CancellationToken);

        _session.Snapshot.State.Should().Be(CallState.Active);
        _session.Snapshot.AcceptedAt.Should().Be(acceptedAt);
        _media.Joins.Should().ContainSingle();
    }

    [Fact]
    public async Task ReconcileAsync_Should_RejoinMedia_When_ServerHasActiveCall()
    {
        _hub.GetCurrentCallAsync().Returns(Info(_callId, CallRole.Caller, CallState.Active));

        await _coordinator.ReconcileAsync(TestContext.Current.CancellationToken);

        _media.Joins.Should().ContainSingle();
        _session.Snapshot.CallId.Should().Be(_callId);
        await _hub.DidNotReceive().EndCallAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task ReconcileAsync_Should_RingWithoutJoiningMedia_When_ServerCallIsRingingForReceiver()
    {
        _hub.GetCurrentCallAsync().Returns(Info(_callId, CallRole.Receiver, CallState.Ringing));

        await _coordinator.ReconcileAsync(TestContext.Current.CancellationToken);

        _session.Snapshot.CallId.Should().Be(_callId);
        _session.Snapshot.IsIncoming.Should().BeTrue();
        _soundPlayer.Playbacks.Should().ContainSingle();
        _media.Joins.Should().BeEmpty();
    }

    [Fact]
    public async Task ReconcileAsync_Should_TearDownLocalCall_When_ServerHasNoCall()
    {
        await JoinActiveCallAsync(_callId);
        _hub.GetCurrentCallAsync().Returns((CallInfo?)null);

        await _coordinator.ReconcileAsync(TestContext.Current.CancellationToken);

        _session.Snapshot.Should().Be(CallSnapshot.Empty);
        _media.LeaveCount.Should().Be(1);
    }

    [Fact]
    public async Task ReconcileAsync_Should_ReplaceLocalCall_When_ServerHasDifferentCall()
    {
        await JoinActiveCallAsync(_callId);
        var serverCallId = Guid.NewGuid();
        _hub.GetCurrentCallAsync().Returns(Info(serverCallId, CallRole.Caller, CallState.Active));

        await _coordinator.ReconcileAsync(TestContext.Current.CancellationToken);

        _session.Snapshot.CallId.Should().Be(serverCallId);
        _media.LeaveCount.Should().Be(1);
        _media.Joins.Should().HaveCount(2);
    }

    private async Task JoinActiveCallAsync(Guid callId)
    {
        _session.SetCall(Info(callId, CallRole.Receiver, CallState.Active));
        await _session.RunAsync(
            () => _session.EnsureMediaAsync(TestContext.Current.CancellationToken),
            TestContext.Current.CancellationToken);
    }

    private static CallInfo Info(Guid callId, CallRole role, CallState state) =>
        new(callId, Guid.NewGuid(), "remote", role, state, null);
}
