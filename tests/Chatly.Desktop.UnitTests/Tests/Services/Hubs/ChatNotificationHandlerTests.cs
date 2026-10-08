using Chatly.Contracts.Features.FriendRequests.Endpoints.AcceptFriendRequest;
using Chatly.Contracts.Features.Friendships.Endpoints.RemoveFriendship;
using Chatly.Contracts.Features.Friendships.Models;
using Chatly.Contracts.Features.Hubs.Abstraction;
using Chatly.Contracts.Features.Hubs.Notifications.NotificationHubServer;
using Chatly.Contracts.Features.Messages.Endpoints.RemoveMessage;
using Chatly.Contracts.Features.Messages.Endpoints.SendMessage;
using Chatly.Contracts.Features.Messages.Models;
using Chatly.Contracts.Features.Reactions.Models;
using Chatly.Contracts.Features.Reactions.Notifications;
using Chatly.Desktop.Abstraction.Navigation;
using Chatly.Desktop.Abstraction.Storage;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Models;
using Chatly.Desktop.Models.UserSession;
using Chatly.Desktop.Services.Api.Clients;
using Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;
using Chatly.Desktop.Services.Api.Refit;
using Chatly.Desktop.Services.Calls;
using Chatly.Desktop.Services.Friendships;
using Chatly.Desktop.Services.Storage;
using Chatly.Desktop.UnitTests.Infrastructure;
using Chatly.Desktop.UnitTests.Tests.Services.Calls.TestDoubles;
using Chatly.Desktop.ViewModels;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using Chatly.Desktop.ViewModels.Pages.ChatPage.Messages;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chatly.Desktop.UnitTests.Tests.Services.Hubs;

public sealed class ChatNotificationHandlerTests : TestBase
{
    private readonly IChatlyApi _api = Substitute.For<IChatlyApi>();
    private readonly Guid _currentUserId = Guid.NewGuid();
    private readonly DirectChatState _directChats = new();
    private readonly FriendState _friends = new();
    private readonly FakeNotificationSoundPlayer _sound = new();
    private readonly IToastService _toasts = Substitute.For<IToastService>();
    private readonly UserRegistry _users = new();
    private ChatMessagesViewModel _messages = null!;
    private ChatPageViewModel _page = null!;
    private ChatSidebarViewModel _sidebar = null!;
    private ChatTypingViewModel _typing = null!;

    private void CreateViewModels()
    {
        var hub = Substitute.For<ICallingHubServer>();
        var session = CallSessionFactory.Create(hub, new FakeCallMediaHost(), _sound);
        var sessionContext = new UserSessionContext(_users);
        _sidebar = new ChatSidebarViewModel(Substitute.For<INavigationService>(), _directChats);
        _messages = new ChatMessagesViewModel(
            new MessageApiClient(_api),
            new ReactionApiClient(_api),
            Substitute.For<IFilePicker>(),
            new FileDownloadClient(),
            _toasts);
        _typing = new ChatTypingViewModel(
            Substitute.For<INotificationHubServer>(),
            NullLogger<ChatTypingViewModel>.Instance);
        _page = new ChatPageViewModel(
            Substitute.For<IFilePicker>(),
            _sidebar,
            new ChatApiClient(_api),
            new MessageApiClient(_api),
            _messages,
            _typing,
            new CallViewModel(
                new CallCoordinator(hub, session, NullLogger<CallCoordinator>.Instance),
                _toasts,
                NullLogger<CallViewModel>.Instance,
                sessionContext,
                _users),
            sessionContext,
            _toasts,
            NullLogger<ChatPageViewModel>.Instance);
    }

    [Fact]
    public Task IncomingMessage_Should_ShowMessageAndMarkRead_When_ChatIsOpen() => UiThread.RunAsync(async () =>
    {
        CreateViewModels();
        var chatId = Guid.NewGuid();
        _messages.BeginConversation(chatId, _currentUserId, "friend", CurrentCancellationToken);
        await _typing.CurrentChatChangedAsync(chatId);
        _typing.ReceiveTypingStatus(new TypingStatusChangedNotification(chatId, true));

        await CreateIncomingMessageHandler().HandleAsync(new IncomingMessageNotification(Message(chatId)));

        _messages.Messages.Should().ContainSingle();
        _typing.IsOtherUserTyping.Should().BeFalse();
        await _api.Received(1).MarkChatReadAsync(chatId, Arg.Any<CancellationToken>());
        _sound.NotificationCount.Should().Be(1);
    });

    [Fact]
    public Task IncomingMessage_Should_IncreaseUnreadCount_When_ChatIsNotOpen() => UiThread.RunAsync(async () =>
    {
        CreateViewModels();
        var chat = new DirectChat { Id = Guid.NewGuid(), User = new User { Id = Guid.NewGuid(), Username = "friend" } };
        _directChats.Set([chat]);

        await CreateIncomingMessageHandler().HandleAsync(new IncomingMessageNotification(Message(chat.Id)));

        chat.UnreadMessageCount.Should().Be(1);
        _messages.Messages.Should().BeEmpty();
        await _api.DidNotReceiveWithAnyArgs().MarkChatReadAsync(default, default);
        _sound.NotificationCount.Should().Be(1);
    });

    [Fact]
    public Task MessageDeleted_Should_MarkMessageDeleted_When_MessageIsShown() => UiThread.RunAsync(async () =>
    {
        CreateViewModels();
        var chatId = Guid.NewGuid();
        var message = Message(chatId);
        _messages.BeginConversation(chatId, _currentUserId, "friend", CurrentCancellationToken);
        _messages.ReceiveIncomingMessage(new IncomingMessageNotification(message));

        await new MessageDeletedNotificationHandler(_messages)
            .HandleAsync(new MessageDeletedNotification(chatId, message.MessageId));

        _messages.Messages.Single().IsDeleted.Should().BeTrue();
    });

    [Fact]
    public Task ReactionChanged_Should_AddReaction_When_MessageIsShown() => UiThread.RunAsync(async () =>
    {
        CreateViewModels();
        var chatId = Guid.NewGuid();
        var message = Message(chatId);
        _messages.BeginConversation(chatId, _currentUserId, "friend", CurrentCancellationToken);
        _messages.ReceiveIncomingMessage(new IncomingMessageNotification(message));

        await new ReactionChangedNotificationHandler(_messages).HandleAsync(new ReactionChangedNotification(
            chatId,
            message.MessageId,
            Guid.NewGuid(),
            message.SenderUserId,
            ReactionType.Like,
            false));

        _messages.Messages.Single().Reactions.Should().ContainSingle();
    });

    [Fact]
    public Task TypingStatusChanged_Should_ShowIndicator_When_ChatIsOpen() => UiThread.RunAsync(async () =>
    {
        CreateViewModels();
        var chatId = Guid.NewGuid();
        await _typing.CurrentChatChangedAsync(chatId);

        await new TypingStatusChangedNotificationHandler(_typing)
            .HandleAsync(new TypingStatusChangedNotification(chatId, true));

        _typing.IsOtherUserTyping.Should().BeTrue();
    });

    [Fact]
    public Task FriendRequestAccepted_Should_AddFriendAndChat_When_ReceiverAccepts() => UiThread.RunAsync(async () =>
    {
        CreateViewModels();
        var friendship = new FriendshipContract(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "friend", null, true);

        await new FriendRequestAcceptedNotificationHandler(CreateFriendshipStateService())
            .HandleAsync(new FriendRequestAcceptedNotification(friendship));

        _friends.Items.Should().ContainSingle().Which.User.Id.Should().Be(friendship.FriendUserId);
        _directChats.Items.Should().ContainSingle().Which.Id.Should().Be(friendship.DirectChatId);
        _friends.OnlineFriends.Should().ContainSingle();
    });

    [Fact]
    public Task FriendshipRemoved_Should_RemoveFriendAndChatAndNotify_When_FriendRemovesUser() =>
        UiThread.RunAsync(async () =>
        {
            CreateViewModels();
            var friendship =
                new FriendshipContract(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "friend", null, false);
            var service = CreateFriendshipStateService();
            service.ApplyAccepted(friendship);

            await new FriendshipRemovedNotificationHandler(service, _toasts)
                .HandleAsync(new FriendshipRemovedNotification(friendship.FriendUserId));

            _friends.Items.Should().BeEmpty();
            _directChats.Items.Should().BeEmpty();
            _toasts.ReceivedWithAnyArgs(1).AddToast(default!);
        });

    private IncomingMessageNotificationHandler CreateIncomingMessageHandler() =>
        new(_sidebar, _page, _messages, _typing, _sound);

    private FriendshipStateService CreateFriendshipStateService() =>
        new(_users, _friends, _directChats, _page);

    private static MessageContract Message(Guid chatId) =>
        new(Guid.NewGuid(), chatId, Guid.NewGuid(), "hello", DateTimeOffset.UtcNow, false, [], []);
}