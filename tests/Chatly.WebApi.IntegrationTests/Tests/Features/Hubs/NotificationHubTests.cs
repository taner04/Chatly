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
        var stopped =
            await listener.ReceiveAsync<TypingStatusChangedNotification>(notification => !notification.IsTyping);

        started.ChatId.Should().Be(chatId.Value);
        started.IsTyping.Should().BeTrue();
        stopped.ChatId.Should().Be(chatId.Value);
    }

    [Fact]
    public async Task StartTyping_Should_Fail_When_UserIsNotPartOfTheChat()
    {
        var first = await CreateUserAsync("first");
        var second = await CreateUserAsync("second");
        var chatId = await CreateFriendshipAsync(first, second);
        await using var intruder = await ConnectNotificationHubAsync();

        await intruder.Awaiting(client => client.StartTypingAsync(chatId.Value))
            .Should().ThrowExactlyAsync<HubException>();
    }

    [Fact]
    public async Task Connect_Should_NotifyFriendsAboutOnlineStatus_When_UserComesOnlineAndLeaves()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var listener = await ConnectNotificationHubAsync(friend);

        var user = await ConnectNotificationHubAsync();
        var online =
            await listener.ReceiveAsync<OnlineStatusChangedNotification>(notification => notification.IsOnline);
        await user.DisposeAsync();
        var offline =
            await listener.ReceiveAsync<OnlineStatusChangedNotification>(notification => !notification.IsOnline);

        online.UserId.Should().Be(CurrentUser.Id.Value);
        offline.UserId.Should().Be(CurrentUser.Id.Value);
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

        listener.ReceivedSoFar<OnlineStatusChangedNotification>()
            .Should().NotContain(notification => !notification.IsOnline);
        var friendships = await CreateAuthenticatedClient(friend).GetFriendshipsAsync(CurrentCancellationToken);
        friendships.Content!.Should().ContainSingle().Subject.IsOnline.Should().BeTrue();
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

        incoming.Message.MessageId.Should().Be(sent.Content.MessageId);
        incoming.Message.Content.Should().Be("ping");
        deleted.MessageId.Should().Be(sent.Content.MessageId);
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

        notification.Request.SenderUserId.Should().Be(CurrentUser.Id.Value);
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

        await client.AcceptFriendRequestAsync(pending.Content!.Items.Should().ContainSingle().Subject.FriendRequestId,
            CurrentCancellationToken);
        var notification = await senderHub.ReceiveAsync<FriendRequestAcceptedNotification>();

        notification.Friendship.FriendUserId.Should().Be(CurrentUser.Id.Value);
    }

    [Fact]
    public async Task RemoveFriendship_Should_PushRemovalToFormerFriend_When_FriendIsConnected()
    {
        var friend = await CreateUserAsync("friend");
        await CreateFriendshipAsync(CurrentUser, friend);
        await using var friendHub = await ConnectNotificationHubAsync(friend);

        await CreateAuthenticatedClient().RemoveFriendshipAsync(friend.Id.Value, CurrentCancellationToken);
        var notification = await friendHub.ReceiveAsync<FriendshipRemovedNotification>();

        notification.AssociatedUserId.Should().Be(CurrentUser.Id.Value);
    }

    [Fact]
    public async Task SetReaction_Should_PushReactionToOtherParticipant_When_ParticipantIsConnected()
    {
        var friend = await CreateUserAsync("friend");
        var chatId = await CreateFriendshipAsync(CurrentUser, friend);
        var sent = await CreateAuthenticatedClient(friend)
            .SendMessageAsync(chatId.Value, "news", CurrentCancellationToken);
        await using var friendHub = await ConnectNotificationHubAsync(friend);

        await CreateAuthenticatedClient().SetReactionAsync(
            sent.Content!.MessageId,
            new SetReactionRequest(ReactionType.Celebrate),
            CurrentCancellationToken);
        var notification = await friendHub.ReceiveAsync<ReactionChangedNotification>();

        notification.MessageId.Should().Be(sent.Content.MessageId);
        notification.UserId.Should().Be(CurrentUser.Id.Value);
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

        notification.UserId.Should().Be(CurrentUser.Id.Value);
        notification.Username.Should().Be("renamed");
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
        lastSeenAt.Should().NotBeNull();
    }
}