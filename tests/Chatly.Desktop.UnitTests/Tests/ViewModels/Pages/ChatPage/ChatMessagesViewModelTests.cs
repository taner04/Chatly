using Chatly.Contracts.Features.Messages.Endpoints.RemoveMessage;
using Chatly.Contracts.Features.Messages.Endpoints.SendMessage;
using Chatly.Contracts.Features.Messages.Models;
using Chatly.Contracts.Features.Reactions.Models;
using Chatly.Contracts.Features.Reactions.Notifications;
using Chatly.Desktop.Abstraction.Storage;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Services.Api.Clients;
using Chatly.Desktop.Services.Api.Refit;
using Chatly.Desktop.Services.Storage;
using Chatly.Desktop.ViewModels.Pages.ChatPage.Messages;

namespace Chatly.Desktop.UnitTests.Tests.ViewModels.Pages.ChatPage;

public sealed class ChatMessagesViewModelTests
{
    private readonly Guid _chatId = Guid.NewGuid();
    private readonly Guid _currentUserId = Guid.NewGuid();
    private readonly Guid _friendId = Guid.NewGuid();

    [Fact]
    public void ReceiveIncomingMessage_Should_AddMessage_When_MessageBelongsToOpenChat()
    {
        var viewModel = CreateOpenConversation();

        var added = viewModel.ReceiveIncomingMessage(new IncomingMessageNotification(Message(_chatId, _friendId)));

        added.Should().BeTrue();
        viewModel.Messages.Should().ContainSingle().Which.IsOwnMessage.Should().BeFalse();
        viewModel.HasMessages.Should().BeTrue();
    }

    [Fact]
    public void ReceiveIncomingMessage_Should_Ignore_When_MessageBelongsToAnotherChat()
    {
        var viewModel = CreateOpenConversation();

        var added = viewModel.ReceiveIncomingMessage(
            new IncomingMessageNotification(Message(Guid.NewGuid(), _friendId)));

        added.Should().BeFalse();
        viewModel.Messages.Should().BeEmpty();
    }

    [Fact]
    public void ReceiveReactionChanged_Should_AddAndRemoveReaction_When_ReactionEventsArrive()
    {
        var viewModel = CreateOpenConversation();
        var message = Message(_chatId, _currentUserId);
        viewModel.ReceiveIncomingMessage(new IncomingMessageNotification(message));
        var reactionId = Guid.NewGuid();

        viewModel.ReceiveReactionChanged(new ReactionChangedNotification(_chatId, message.MessageId, reactionId,
            _friendId, ReactionType.Fire, false));
        var afterAdd = viewModel.Messages.Single().Reactions.Count;
        viewModel.ReceiveReactionChanged(new ReactionChangedNotification(_chatId, message.MessageId, reactionId,
            _friendId, ReactionType.Fire, true));

        afterAdd.Should().Be(1);
        viewModel.Messages.Single().Reactions.Should().BeEmpty();
    }

    [Fact]
    public void ReceiveMessageDeleted_Should_MarkMessageDeleted_When_MessageIsInOpenChat()
    {
        var viewModel = CreateOpenConversation();
        var message = Message(_chatId, _currentUserId);
        viewModel.ReceiveIncomingMessage(new IncomingMessageNotification(message));

        viewModel.ReceiveMessageDeleted(new MessageDeletedNotification(_chatId, message.MessageId));

        var item = viewModel.Messages.Single();
        item.IsDeleted.Should().BeTrue();
        item.CanRemoveMessage.Should().BeFalse();
        item.CanReact.Should().BeFalse();
    }

    [Fact]
    public void AddSentMessage_Should_Ignore_When_ConversationChangedMeanwhile()
    {
        var viewModel = CreateOpenConversation();
        var staleVersion = viewModel.ConversationVersion;
        viewModel.BeginConversation(Guid.NewGuid(), _currentUserId, "other", TestContext.Current.CancellationToken);

        var added = viewModel.AddSentMessage(Message(_chatId, _currentUserId), staleVersion);

        added.Should().BeFalse();
        viewModel.Messages.Should().BeEmpty();
    }

    private ChatMessagesViewModel CreateOpenConversation()
    {
        var api = Substitute.For<IChatlyApi>();
        var viewModel = new ChatMessagesViewModel(
            new MessageApiClient(api),
            new ReactionApiClient(api),
            Substitute.For<IFilePicker>(),
            new FileDownloadClient(),
            Substitute.For<IToastService>());
        viewModel.BeginConversation(_chatId, _currentUserId, "friend", TestContext.Current.CancellationToken);
        return viewModel;
    }

    private static MessageContract Message(Guid chatId, Guid senderId) =>
        new(Guid.NewGuid(), chatId, senderId, "hello", DateTimeOffset.UtcNow, false, [], []);
}