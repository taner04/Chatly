using System.Collections.ObjectModel;
using Chatly.Contracts.Features.Messages.Endpoints.GetMessages;
using Chatly.Contracts.Features.Messages.Endpoints.RemoveMessage;
using Chatly.Contracts.Features.Messages.Endpoints.SendMessage;
using Chatly.Contracts.Features.Messages.Models;
using Chatly.Contracts.Features.Reactions.Models;
using Chatly.Contracts.Features.Reactions.Notifications;
using Chatly.Desktop.Abstraction.Storage;
using Chatly.Desktop.Services.Api.Clients;
using Chatly.Desktop.Services.Storage;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage.Messages;

[SingletonService]
public sealed partial class ChatMessagesViewModel(
    MessageApiClient messageApiClient,
    ReactionApiClient reactionApiClient,
    IFilePicker filePicker,
    FileDownloadClient fileDownloadClient,
    IToastService toastService) : ObservableObject
{
    private CancellationTokenSource? _conversationCancellation;
    private Guid? _currentChatId;
    private Guid _currentUserId;
    private Guid? _nextBeforeMessageId;
    private DateTimeOffset? _nextBeforeSentAt;
    private string _otherUserName = string.Empty;

    public ObservableCollection<ChatMessageViewModel> Messages { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyChat))]
    public partial bool IsLoadingMessages { get; private set; }

    [ObservableProperty] public partial bool HasOlderMessages { get; private set; }

    public bool HasMessages => Messages.Count > 0;

    public bool IsEmptyChat => _currentChatId is not null && !IsLoadingMessages && !HasMessages;

    internal int ConversationVersion { get; private set; }

    internal int BeginConversation(
        Guid chatId,
        Guid currentUserId,
        string otherUserName,
        CancellationToken cancellationToken)
    {
        if (currentUserId == Guid.Empty)
        {
            throw new ArgumentException("A valid current-user ID is required.", nameof(currentUserId));
        }

        ResetConversation();
        _currentChatId = chatId;
        _currentUserId = currentUserId;
        _otherUserName = otherUserName;
        _conversationCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        OnPropertyChanged(nameof(IsEmptyChat));
        return ConversationVersion;
    }

    internal async Task LoadInitialMessagesAsync(int conversationVersion)
    {
        if (_currentChatId is not { } chatId ||
            _conversationCancellation is not { } cancellation)
        {
            return;
        }

        await LoadMessagesAsync(chatId, conversationVersion, cancellation.Token);
    }

    internal bool IsCurrentConversation(Guid chatId, int conversationVersion) =>
        conversationVersion == ConversationVersion && _currentChatId == chatId;

    internal bool AddSentMessage(MessageContract message, int conversationVersion)
    {
        if (!IsCurrentConversation(message.ChatId, conversationVersion))
        {
            return false;
        }

        AddMessage(message);
        return true;
    }

    internal bool ReceiveIncomingMessage(IncomingMessageNotification notification)
    {
        var message = notification.Message;

        if (_currentChatId != message.ChatId)
        {
            return false;
        }

        AddMessage(message);
        return true;
    }

    internal void ReceiveReactionChanged(ReactionChangedNotification message)
    {
        if (_currentChatId != message.ChatId)
        {
            return;
        }

        var chatMessage = Messages.FirstOrDefault(item => item.MessageId == message.MessageId);
        if (chatMessage is null)
        {
            return;
        }

        if (message.IsRemoved)
        {
            chatMessage.RemoveReaction(message.ReactionId);
            return;
        }

        chatMessage.UpsertReaction(new MessageReactionContract(
            message.ReactionId,
            message.UserId,
            message.ReactionType));
    }

    internal void ReceiveMessageDeleted(MessageDeletedNotification message)
    {
        if (_currentChatId != message.ChatId)
        {
            return;
        }

        Messages.FirstOrDefault(item => item.MessageId == message.MessageId)?.MarkDeleted();
    }

    internal void Reset()
    {
        ResetConversation();
        OnPropertyChanged(nameof(IsEmptyChat));
    }

    [RelayCommand]
    private async Task LoadOlderMessagesAsync(CancellationToken cancellationToken)
    {
        if (IsLoadingMessages ||
            !HasOlderMessages ||
            _currentChatId is not { } chatId ||
            _conversationCancellation is not { } conversationCancellation)
        {
            return;
        }

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            conversationCancellation.Token);
        await LoadMessagesAsync(chatId, ConversationVersion, linkedCancellation.Token);
    }

    private async Task LoadMessagesAsync(
        Guid chatId,
        int conversationVersion,
        CancellationToken cancellationToken)
    {
        IsLoadingMessages = true;

        try
        {
            var result = await messageApiClient.GetMessagesAsync(
                new GetMessagesRequest(chatId, _nextBeforeSentAt, _nextBeforeMessageId),
                cancellationToken);

            if (!IsCurrentConversation(chatId, conversationVersion))
            {
                return;
            }

            if (result.IsFailure)
            {
                toastService.ShowError(result.Error);
                return;
            }

            var existingMessageIds = Messages.Select(message => message.MessageId).ToHashSet();
            var insertIndex = 0;

            foreach (var message in result.Value.Items)
            {
                if (existingMessageIds.Add(message.MessageId))
                {
                    Messages.Insert(insertIndex++, CreateMessageViewModel(message));
                }
            }

            _nextBeforeSentAt = result.Value.NextBeforeSentAt;
            _nextBeforeMessageId = result.Value.NextBeforeMessageId;
            HasOlderMessages = result.Value.HasMore;
            NotifyMessagesChanged();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            if (conversationVersion == ConversationVersion)
            {
                IsLoadingMessages = false;
            }
        }
    }

    private void AddMessage(MessageContract message)
    {
        if (Messages.Any(existing => existing.MessageId == message.MessageId))
        {
            return;
        }

        Messages.Add(CreateMessageViewModel(message));
        NotifyMessagesChanged();
    }

    private ChatMessageViewModel CreateMessageViewModel(MessageContract message) =>
        new(
            message,
            _currentUserId,
            _otherUserName,
            messageApiClient,
            reactionApiClient,
            filePicker,
            fileDownloadClient,
            toastService,
            _conversationCancellation?.Token ?? throw new InvalidOperationException(
                "A conversation must be active before constructing message rows."));

    private void ResetConversation()
    {
        ConversationVersion++;
        _conversationCancellation?.Cancel();

        foreach (var message in Messages)
        {
            message.Dispose();
        }

        Messages.Clear();
        _conversationCancellation?.Dispose();
        _conversationCancellation = null;
        _currentChatId = null;
        _currentUserId = Guid.Empty;
        _otherUserName = string.Empty;
        _nextBeforeSentAt = null;
        _nextBeforeMessageId = null;
        HasOlderMessages = false;
        IsLoadingMessages = false;

        NotifyMessagesChanged();
    }

    private void NotifyMessagesChanged()
    {
        OnPropertyChanged(nameof(HasMessages));
        OnPropertyChanged(nameof(IsEmptyChat));
    }
}