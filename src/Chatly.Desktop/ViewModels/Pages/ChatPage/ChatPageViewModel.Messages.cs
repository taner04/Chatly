using System.Linq;
using Chatly.Contracts.Endpoints.Messages.Requests;
using Chatly.Contracts.Endpoints.Messages.Results;
using Chatly.Desktop.Extensions;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage;

public sealed partial class ChatPageViewModel
{
    [RelayCommand]
    private async Task LoadOlderMessagesAsync(CancellationToken cancellationToken)
    {
        if (IsLoadingMessages || !HasOlderMessages || CurrentChat?.DirectChatId is null)
        {
            return;
        }

        await LoadMessagesAsync(CurrentChat.DirectChatId.Value, _conversationVersion, cancellationToken);
    }

    public bool ReceiveIncomingMessage(IncomingChatMessage message)
    {
        if (CurrentChat?.DirectChatId != message.ChatId)
        {
            return false;
        }

        AddMessage(new GetMessagesItem(
            message.MessageId,
            message.ChatId,
            message.SenderUserId,
            message.Content,
            message.SentAt));
        ClearOtherUserTyping();
        return true;
    }

    private async Task LoadMessagesAsync(
        Guid chatId,
        int version,
        CancellationToken cancellationToken)
    {
        IsLoadingMessages = true;

        try
        {
            var result = await messageWebService.GetMessagesAsync(
                new GetMessagesRequest(
                    chatId,
                    _nextBeforeSentAt,
                    _nextBeforeMessageId),
                cancellationToken);

            if (version != _conversationVersion || CurrentChat?.DirectChatId != chatId)
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
        finally
        {
            if (version == _conversationVersion)
            {
                IsLoadingMessages = false;
            }
        }
    }

    private void AddMessage(GetMessagesItem message)
    {
        if (Messages.Any(existing => existing.MessageId == message.MessageId))
        {
            return;
        }

        Messages.Add(CreateMessageViewModel(message));
        NotifyMessagesChanged();
    }

    private ChatMessageViewModel CreateMessageViewModel(GetMessagesItem message)
    {
        return new ChatMessageViewModel(
            message.MessageId,
            message.SenderUserId,
            message.Content,
            message.SentAt,
            message.SenderUserId == userSessionContext.CurrentUser?.Id);
    }

    private void NotifyMessagesChanged()
    {
        OnPropertyChanged(nameof(HasMessages));
        OnPropertyChanged(nameof(IsEmptyChat));
    }
}