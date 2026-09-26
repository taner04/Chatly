using Chatly.Contracts.Features.Hubs;
using Chatly.Contracts.Features.Hubs.Notifications.CallSignalingHubServer;
using Chatly.WebApi.Features.Calls.Enums;
using Chatly.WebApi.Features.Calls.Models;
using Microsoft.AspNetCore.SignalR;
using CallEndReason = Chatly.Contracts.Features.Hubs.CallEndReason;
using StoredCallEndReason = Chatly.WebApi.Features.Calls.Enums.CallEndReason;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Calls;

public sealed class CallingHubTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task StartCall_Should_Fail_When_UsersAreNotFriends()
    {
        var stranger = await CreateUserAsync("stranger");
        await using var caller = await ConnectCallHubAsync();

        var exception = await Assert.ThrowsAsync<HubException>(() => caller.StartCallAsync(stranger.Id.Value));

        Assert.Contains("friends", exception.Message);
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

        Assert.Equal(CallState.Ringing, started.State);
        Assert.Equal(started.CallId, incoming.CallId);
        Assert.Equal(CurrentUser.Id.Value, incoming.RemoteUserId);
        Assert.Equal(CallState.Active, accepted.State);
        Assert.NotNull(accepted.AcceptedAt);
        Assert.Equal(accepted.AcceptedAt, acceptedNotification.AcceptedAt);
        Assert.False(string.IsNullOrWhiteSpace(media.Token));
        Assert.Equal(CallEndReason.Completed, ended.Reason);

        await using var dbContext = GetDbContext();
        var call = await dbContext.Calls.SingleAsync(CurrentCancellationToken);
        Assert.Equal(CallStatus.Ended, call.Status);
        Assert.False(await dbContext.ActiveCallParticipants.AnyAsync(CurrentCancellationToken));
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

        var exception = await Assert.ThrowsAsync<HubException>(() => otherCaller.StartCallAsync(friend.Id.Value));

        Assert.Contains("already in a call", exception.Message);
    }

    [Fact]
    public async Task JoinMedia_Should_Fail_When_CallIsStillRinging()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        var started = await caller.StartCallAsync(friend.Id.Value);

        await Assert.ThrowsAsync<HubException>(() => caller.JoinMediaAsync(started.CallId));
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

        Assert.Equal(CallEndReason.Declined, rejected.Reason);
        Assert.Null(await caller.GetCurrentCallAsync());
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

        Assert.Equal(CallEndReason.Cancelled, ended.Reason);
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
        var changed = await receiverLaptop.ReceiveAsync<CallStateChangedNotification>(
            notification => notification.State == CallState.Active);

        Assert.Equal(started.CallId, changed.CallId);
        Assert.NotNull(changed.AcceptedAt);
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

        Assert.Equal(CallRole.Caller, callerView!.Role);
        Assert.Equal(friend.Id.Value, callerView.RemoteUserId);
        Assert.Equal(CallRole.Receiver, receiverView!.Role);
        Assert.Equal(CallState.Active, receiverView.State);
        Assert.Equal(callerView.AcceptedAt, receiverView.AcceptedAt);
    }

    [Fact]
    public async Task StartCall_Should_Fail_When_UserCallsThemselves()
    {
        await using var caller = await ConnectCallHubAsync();

        var exception = await Assert.ThrowsAsync<HubException>(() => caller.StartCallAsync(CurrentUser.Id.Value));

        Assert.Contains("themselves", exception.Message);
    }

    [Fact]
    public async Task AcceptCall_Should_FailAndKeepRinging_When_CallerAcceptsOwnCall()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        var started = await caller.StartCallAsync(friend.Id.Value);

        await Assert.ThrowsAsync<HubException>(() => caller.AcceptCallAsync(started.CallId));

        await AssertStoredCallAsync(CallStatus.Ringing, null);
    }

    [Fact]
    public async Task RejectCall_Should_FailAndKeepRinging_When_CallerRejectsOwnCall()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var caller = await ConnectCallHubAsync();
        var started = await caller.StartCallAsync(friend.Id.Value);

        await Assert.ThrowsAsync<HubException>(() => caller.RejectCallAsync(started.CallId));

        await AssertStoredCallAsync(CallStatus.Ringing, null);
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

        Assert.Equal(CallEndReason.Completed, ended.Reason);
        await AssertStoredCallAsync(CallStatus.Ended, StoredCallEndReason.Completed);
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

        await Assert.ThrowsAsync<HubException>(() => receiver.EndCallAsync(started.CallId));

        await AssertStoredCallAsync(CallStatus.Ended, StoredCallEndReason.Completed);
        Assert.Equal(endedAt, await GetStoredEndedAtAsync());
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

        await Assert.ThrowsAsync<HubException>(() => intruder.EndCallAsync(started.CallId));
        await Assert.ThrowsAsync<HubException>(() => intruder.JoinMediaAsync(started.CallId));

        await AssertStoredCallAsync(CallStatus.Active, null);
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

        await Assert.ThrowsAsync<HubException>(() => receiver.JoinMediaAsync(started.CallId));
    }

    private async Task AssertStoredCallAsync(CallStatus status, StoredCallEndReason? reason)
    {
        await using var dbContext = GetDbContext();
        var call = await dbContext.Calls.SingleAsync(CurrentCancellationToken);
        Assert.Equal(status, call.Status);
        Assert.Equal(reason, call.EndReason);
    }

    private async Task<DateTimeOffset?> GetStoredEndedAtAsync()
    {
        await using var dbContext = GetDbContext();
        var call = await dbContext.Calls.SingleAsync(CurrentCancellationToken);
        return call.EndedAt;
    }
}
