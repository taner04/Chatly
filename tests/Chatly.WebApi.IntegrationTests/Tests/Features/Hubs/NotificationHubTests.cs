using Chatly.Contracts.Features.FriendRequests.Endpoints.AcceptFriendRequest;
using Chatly.Contracts.Features.FriendRequests.Endpoints.SendFriendRequest;
using Chatly.Contracts.Features.Friendships.Endpoints.RemoveFriendship;
using Chatly.Contracts.Features.Hubs.Notifications.NotificationHubServer;
using Chatly.Contracts.Features.Messages.Endpoints.RemoveMessage;
using Chatly.Contracts.Features.Messages.Endpoints.SendMessage;
using Chatly.Contracts.Features.Reactions.Endpoints.SetReaction;
using Chatly.Contracts.Features.Reactions.Models;
using Chatly.Contracts.Features.Reactions.Notifications;
using Chatly.Contracts.Features.Users.Endpoints.UpdateUsername;
using Chatly.Contracts.Features.Users.Notifications;
using Chatly.WebApi.Common.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.IntegrationTests.Tests.Features.Hubs;

public sealed class NotificationHubTests(TestingFixture fixture) : TestingBase(fixture)
{
    [Fact]
    public async Task StartTyping_Should_NotifyOtherParticipant_When_UserTypesInSharedChat()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        await using var typer = await ConnectNotificationHubAsync();
        await using var listener = await ConnectNotificationHubAsync(friend);

        await typer.StartTypingAsync(chatId.Value);
        var started = await listener.ReceiveAsync<TypingStatusChangedNotification>();
        await typer.StopTypingAsync(chatId.Value);
        var stopped = await listener.ReceiveAsync<TypingStatusChangedNotification>(notification => !notification.IsTyping);

        Assert.Equal(chatId.Value, started.ChatId);
        Assert.True(started.IsTyping);
        Assert.Equal(chatId.Value, stopped.ChatId);
    }

    [Fact]
    public async Task StartTyping_Should_Fail_When_UserIsNotPartOfTheChat()
    {
        var first = await CreateUserAsync("first");
        var second = await CreateUserAsync("second");
        var chatId = await CreateFriendshipAsync(first, second);
        await using var intruder = await ConnectNotificationHubAsync();

        await Assert.ThrowsAsync<HubException>(() => intruder.StartTypingAsync(chatId.Value));
    }

    [Fact]
    public async Task Connect_Should_NotifyFriendsAboutOnlineStatus_When_UserComesOnlineAndLeaves()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var listener = await ConnectNotificationHubAsync(friend);

        var user = await ConnectNotificationHubAsync();
        var online = await listener.ReceiveAsync<OnlineStatusChangedNotification>(notification => notification.IsOnline);
        await user.DisposeAsync();
        var offline = await listener.ReceiveAsync<OnlineStatusChangedNotification>(notification => !notification.IsOnline);

        Assert.Equal(CurrentUser.Id.Value, online.UserId);
        Assert.Equal(CurrentUser.Id.Value, offline.UserId);
    }

    [Fact]
    public async Task Disconnect_Should_NotNotifyFriendsAboutOffline_When_UserReconnectsWithinGracePeriod()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var listener = await ConnectNotificationHubAsync(friend);
        var first = await ConnectNotificationHubAsync();
        await listener.ReceiveAsync<OnlineStatusChangedNotification>(notification => notification.IsOnline);

        await first.DisposeAsync();
        await using var second = await ConnectNotificationHubAsync();
        await Task.Delay(OnlineStatusPublisher.OfflineGracePeriod + TimeSpan.FromSeconds(2), CurrentCancellationToken);

        Assert.DoesNotContain(listener.ReceivedSoFar<OnlineStatusChangedNotification>(), notification => !notification.IsOnline);
        var friendships = await CreateAuthenticatedClient(friend).GetFriendshipsAsync(CurrentCancellationToken);
        Assert.True(Assert.Single(friendships.Content!).IsOnline);
    }

    [Fact]
    public async Task SendMessage_Should_PushMessageToReceiver_When_ReceiverIsConnected()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        await using var receiver = await ConnectNotificationHubAsync(friend);
        var client = CreateAuthenticatedClient();

        var sent = await client.SendMessageAsync(chatId.Value, "ping", CurrentCancellationToken);
        var incoming = await receiver.ReceiveAsync<IncomingMessageNotification>();
        await client.RemoveMessageAsync(sent.Content!.MessageId, CurrentCancellationToken);
        var deleted = await receiver.ReceiveAsync<MessageDeletedNotification>();

        Assert.Equal(sent.Content.MessageId, incoming.Message.MessageId);
        Assert.Equal("ping", incoming.Message.Content);
        Assert.Equal(sent.Content.MessageId, deleted.MessageId);
    }

    [Fact]
    public async Task SendFriendRequest_Should_PushRequestToReceiver_When_ReceiverIsConnected()
    {
        var receiverUser = await CreateUserAsync("receiver");
        await using var receiver = await ConnectNotificationHubAsync(receiverUser);

        await CreateAuthenticatedClient().SendFriendRequestAsync(
            new SendFriendRequestRequest(receiverUser.Id.Value),
            CurrentCancellationToken);
        var notification = await receiver.ReceiveAsync<IncomingFriendRequestNotification>();

        Assert.Equal(CurrentUser.Id.Value, notification.Request.SenderUserId);
    }

    [Fact]
    public async Task AcceptFriendRequest_Should_PushFriendshipToSender_When_SenderIsConnected()
    {
        var sender = await CreateUserAsync("sender");
        await using var senderHub = await ConnectNotificationHubAsync(sender);
        await CreateAuthenticatedClient(sender).SendFriendRequestAsync(
            new SendFriendRequestRequest(CurrentUser.Id.Value),
            CurrentCancellationToken);
        var client = CreateAuthenticatedClient();
        var pending = await client.GetFriendRequestsAsync(1, 10, CurrentCancellationToken);

        await client.AcceptFriendRequestAsync(Assert.Single(pending.Content!.Items).FriendRequestId, CurrentCancellationToken);
        var notification = await senderHub.ReceiveAsync<FriendRequestAcceptedNotification>();

        Assert.Equal(CurrentUser.Id.Value, notification.Friendship.FriendUserId);
    }

    [Fact]
    public async Task RemoveFriendship_Should_PushRemovalToFormerFriend_When_FriendIsConnected()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var friendHub = await ConnectNotificationHubAsync(friend);

        await CreateAuthenticatedClient().RemoveFriendshipAsync(friend.Id.Value, CurrentCancellationToken);
        var notification = await friendHub.ReceiveAsync<FriendshipRemovedNotification>();

        Assert.Equal(CurrentUser.Id.Value, notification.AssociatedUserId);
    }

    [Fact]
    public async Task SetReaction_Should_PushReactionToOtherParticipant_When_ParticipantIsConnected()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var sent = await CreateAuthenticatedClient(friend).SendMessageAsync(chatId.Value, "news", CurrentCancellationToken);
        await using var friendHub = await ConnectNotificationHubAsync(friend);

        await CreateAuthenticatedClient().SetReactionAsync(
            sent.Content!.MessageId,
            new SetReactionRequest(ReactionType.Celebrate),
            CurrentCancellationToken);
        var notification = await friendHub.ReceiveAsync<ReactionChangedNotification>();

        Assert.Equal(sent.Content.MessageId, notification.MessageId);
        Assert.Equal(CurrentUser.Id.Value, notification.UserId);
    }

    [Fact]
    public async Task UpdateUsername_Should_PushProfileToFriends_When_FriendIsConnected()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var friendHub = await ConnectNotificationHubAsync(friend);

        await CreateAuthenticatedClient().UpdateUsernameAsync(
            new UpdateUsernameRequest("renamed"),
            CurrentCancellationToken);
        var notification = await friendHub.ReceiveAsync<UserProfileUpdatedNotification>();

        Assert.Equal(CurrentUser.Id.Value, notification.UserId);
        Assert.Equal("renamed", notification.Username);
    }

    [Fact]
    public async Task Connect_Should_UpdateLastSeen_When_UserConnects()
    {
        await using (var dbContext = GetDbContext())
        {
            await dbContext.Users
                .Where(user => user.Id == CurrentUser.Id)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(user => user.LastSeenAt, DateTimeOffset.UtcNow.AddDays(-30)),
                    CurrentCancellationToken);
        }

        await using var connection = await ConnectNotificationHubAsync();

        var lastSeenAt = await WaitForAsync(async () =>
        {
            await using var dbContext = GetDbContext();
            var user = await dbContext.Users.SingleAsync(user => user.Id == CurrentUser.Id, CurrentCancellationToken);
            return user.LastSeenAt > DateTimeOffset.UtcNow.AddMinutes(-1) ? user.LastSeenAt : null;
        });
        Assert.NotNull(lastSeenAt);
    }
}
