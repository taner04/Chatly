using Chatly.Contracts.Features.Hubs;
using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;
using Microsoft.AspNetCore.SignalR;
using CallEndReason = Chatly.Contracts.Features.Hubs.CallEndReason;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Calls;

public sealed class CallingHubTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task StartCall_Should_Fail_When_UsersAreNotFriends()
    {
        var stranger = await CreateUserAsync("stranger");
        await using var caller = await ConnectCallHubAsync();

        var exception = (await caller.Awaiting(client => client.StartCallAsync(stranger.Id.Value))
            .Should().ThrowExactlyAsync<HubException>()).Which;

        exception.Message.Should().Contain("friends");
    }

    [Fact]
    public async Task CallLifecycle_Should_RingAcceptJoinMediaAndEnd_When_FriendsCallEachOther()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        await using var receiver = await ConnectCallHubAsync(friend);

        var started = await caller.StartCallAsync(friend.Id.Value);
        var incoming = await receiver.ReceiveAsync<IncomingCallNotification>();
        var accepted = await receiver.AcceptCallAsync(started.CallId);
        var acceptedNotification = await caller.ReceiveAsync<CallAcceptedNotification>();
        var media = await caller.JoinMediaAsync(started.CallId);
        await caller.EndCallAsync(started.CallId);
        var ended = await receiver.ReceiveAsync<CallEndedNotification>();

        started.State.Should().Be(CallState.Ringing);
        incoming.CallId.Should().Be(started.CallId);
        incoming.RemoteUserId.Should().Be(CurrentUser.Id.Value);
        accepted.State.Should().Be(CallState.Active);
        accepted.AcceptedAt.Should().NotBeNull();
        acceptedNotification.AcceptedAt.Should().Be(accepted.AcceptedAt);
        string.IsNullOrWhiteSpace(media.Token).Should().BeFalse();
        ended.Reason.Should().Be(CallEndReason.Completed);

        await using var dbContext = GetDbContext();
        var call = await dbContext.Calls.SingleAsync(CurrentCancellationToken);
        call.Status.Should().Be(CallState.Ended);
        (await dbContext.ActiveCallParticipants.AnyAsync(CurrentCancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task StartCall_Should_Fail_When_ReceiverIsAlreadyInACall()
    {
        var friend = await CreateUserAsync("friend");
        var otherFriend = await CreateUserAsync("other_friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await CreateFriendshipAsync(otherFriend, friend);
        await using var caller = await ConnectCallHubAsync();
        await using var otherCaller = await ConnectCallHubAsync(otherFriend);
        await caller.StartCallAsync(friend.Id.Value);

        var exception = (await otherCaller.Awaiting(client => client.StartCallAsync(friend.Id.Value))
            .Should().ThrowExactlyAsync<HubException>()).Which;

        exception.Message.Should().Contain("already in a call");
    }

    [Fact]
    public async Task JoinMedia_Should_Fail_When_CallIsStillRinging()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        var started = await caller.StartCallAsync(friend.Id.Value);

        await caller.Awaiting(client => client.JoinMediaAsync(started.CallId))
            .Should().ThrowExactlyAsync<HubException>();
    }

    [Fact]
    public async Task RejectCall_Should_EndCallAsDeclinedForBothUsers_When_ReceiverRejects()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        await using var receiver = await ConnectCallHubAsync(friend);
        var started = await caller.StartCallAsync(friend.Id.Value);

        await receiver.RejectCallAsync(started.CallId);
        var rejected = await caller.ReceiveAsync<CallRejectedNotification>();

        rejected.Reason.Should().Be(CallEndReason.Declined);
        (await caller.GetCurrentCallAsync()).Should().BeNull();
    }

    [Fact]
    public async Task EndCall_Should_EndAsCancelled_When_CallerHangsUpWhileRinging()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        await using var receiver = await ConnectCallHubAsync(friend);
        var started = await caller.StartCallAsync(friend.Id.Value);

        await caller.EndCallAsync(started.CallId);
        var ended = await receiver.ReceiveAsync<CallEndedNotification>();

        ended.Reason.Should().Be(CallEndReason.Cancelled);
    }

    [Fact]
    public async Task AcceptCall_Should_InformReceiversOtherDevices_When_OneDeviceAccepts()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        await using var receiverPhone = await ConnectCallHubAsync(friend);
        await using var receiverLaptop = await ConnectCallHubAsync(friend);
        var started = await caller.StartCallAsync(friend.Id.Value);
        await receiverLaptop.ReceiveAsync<IncomingCallNotification>();

        await receiverPhone.AcceptCallAsync(started.CallId);
        var changed =
            await receiverLaptop.ReceiveAsync<CallStateChangedNotification>(notification =>
                notification.State == CallState.Active);

        changed.CallId.Should().Be(started.CallId);
        changed.AcceptedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCurrentCall_Should_ReturnActiveCallForBothUsers_When_CallIsAccepted()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        await using var receiver = await ConnectCallHubAsync(friend);
        var started = await caller.StartCallAsync(friend.Id.Value);
        await receiver.AcceptCallAsync(started.CallId);

        var callerView = await caller.GetCurrentCallAsync();
        var receiverView = await receiver.GetCurrentCallAsync();

        callerView!.Role.Should().Be(CallRole.Caller);
        callerView.RemoteUserId.Should().Be(friend.Id.Value);
        receiverView!.Role.Should().Be(CallRole.Receiver);
        receiverView.State.Should().Be(CallState.Active);
        receiverView.AcceptedAt.Should().Be(callerView.AcceptedAt);
    }

    [Fact]
    public async Task StartCall_Should_Fail_When_UserCallsThemselves()
    {
        await using var caller = await ConnectCallHubAsync();

        var exception = (await caller.Awaiting(client => client.StartCallAsync(CurrentUser.Id.Value))
            .Should().ThrowExactlyAsync<HubException>()).Which;

        exception.Message.Should().Contain("themselves");
    }

    [Fact]
    public async Task AcceptCall_Should_FailAndKeepRinging_When_CallerAcceptsOwnCall()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        var started = await caller.StartCallAsync(friend.Id.Value);

        await caller.Awaiting(client => client.AcceptCallAsync(started.CallId))
            .Should().ThrowExactlyAsync<HubException>();

        await AssertStoredCallAsync(CallState.Ringing, null);
    }

    [Fact]
    public async Task RejectCall_Should_FailAndKeepRinging_When_CallerRejectsOwnCall()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        var started = await caller.StartCallAsync(friend.Id.Value);

        await caller.Awaiting(client => client.RejectCallAsync(started.CallId))
            .Should().ThrowExactlyAsync<HubException>();

        await AssertStoredCallAsync(CallState.Ringing, null);
    }

    [Fact]
    public async Task EndCall_Should_EndAsCompleted_When_ReceiverHangsUpActiveCall()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        await using var receiver = await ConnectCallHubAsync(friend);
        var started = await caller.StartCallAsync(friend.Id.Value);
        await receiver.AcceptCallAsync(started.CallId);

        await receiver.EndCallAsync(started.CallId);
        var ended = await caller.ReceiveAsync<CallEndedNotification>();

        ended.Reason.Should().Be(CallEndReason.Completed);
        await AssertStoredCallAsync(CallState.Ended, CallEndReason.Completed);
    }

    [Fact]
    public async Task EndCall_Should_FailAndKeepOutcome_When_CallAlreadyEnded()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        await using var receiver = await ConnectCallHubAsync(friend);
        var started = await caller.StartCallAsync(friend.Id.Value);
        await receiver.AcceptCallAsync(started.CallId);
        await caller.EndCallAsync(started.CallId);
        var endedAt = await GetStoredEndedAtAsync();

        await receiver.Awaiting(client => client.EndCallAsync(started.CallId))
            .Should().ThrowExactlyAsync<HubException>();

        await AssertStoredCallAsync(CallState.Ended, CallEndReason.Completed);
        (await GetStoredEndedAtAsync()).Should().Be(endedAt);
    }

    [Fact]
    public async Task EndCall_Should_FailAndKeepActive_When_UserIsNotParticipant()
    {
        var friend = await CreateUserAsync("friend");
        var outsider = await CreateUserAsync("outsider");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        await using var receiver = await ConnectCallHubAsync(friend);
        await using var intruder = await ConnectCallHubAsync(outsider);
        var started = await caller.StartCallAsync(friend.Id.Value);
        await receiver.AcceptCallAsync(started.CallId);

        await intruder.Awaiting(client => client.EndCallAsync(started.CallId))
            .Should().ThrowExactlyAsync<HubException>();
        await intruder.Awaiting(client => client.JoinMediaAsync(started.CallId))
            .Should().ThrowExactlyAsync<HubException>();

        await AssertStoredCallAsync(CallState.Active, null);
    }

    [Fact]
    public async Task JoinMedia_Should_Fail_When_CallHasEnded()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        await using var receiver = await ConnectCallHubAsync(friend);
        var started = await caller.StartCallAsync(friend.Id.Value);
        await receiver.AcceptCallAsync(started.CallId);
        await caller.EndCallAsync(started.CallId);

        await receiver.Awaiting(client => client.JoinMediaAsync(started.CallId))
            .Should().ThrowExactlyAsync<HubException>();
    }

    private async Task AssertStoredCallAsync(CallState status, CallEndReason? reason)
    {
        await using var dbContext = GetDbContext();
        var call = await dbContext.Calls.SingleAsync(CurrentCancellationToken);
        call.Status.Should().Be(status);
        call.EndReason.Should().Be(reason);
    }

    private async Task<DateTimeOffset?> GetStoredEndedAtAsync()
    {
        await using var dbContext = GetDbContext();
        var call = await dbContext.Calls.SingleAsync(CurrentCancellationToken);
        return call.EndedAt;
    }
}