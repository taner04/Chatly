using System.Collections.ObjectModel;
using System.Linq;
using Chatly.Contracts.Endpoints.Messages.Requests;
using Chatly.Contracts.Endpoints.Messages.Results;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Extensions;
using Chatly.Desktop.Services.Api;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage;

[SingletonService]
public sealed partial class ChatPageViewModel(
    ChatSidebarViewModel chatSidebar,
    ChatWebService chatWebService,
    MessageWebService messageWebService,
    UserSessionContext userSessionContext,
    IToastService toastService,
    INotificationHubServer notificationHubServer,
    ILogger<ChatPageViewModel> logger)
    : PageViewModelBase, INavigationParameterAware
{
    private const int MessagePageSize = 50;
    private int _conversationVersion;
    private Guid? _nextBeforeMessageId;
    private DateTimeOffset? _nextBeforeSentAt;

    public ObservableCollection<ChatPreviewViewModel> Chats => chatSidebar.Chats;

    public ObservableCollection<ChatMessageViewModel> Messages { get; } = [];

    [ObservableProperty] public partial ChatPreviewViewModel? CurrentChat { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
    public partial string DraftMessage { get; set; } = string.Empty;

    [ObservableProperty] public partial bool IsLoadingMessages { get; private set; }

    [ObservableProperty] public partial bool HasOlderMessages { get; private set; }

    [ObservableProperty] public partial bool IsOtherUserTyping { get; private set; }

    public bool HasCurrentChat => CurrentChat is not null;

    public bool HasMessages => Messages.Count > 0;

    public bool IsEmptyChat => HasCurrentChat && !IsLoadingMessages && !HasMessages;

    public async Task OnNavigatedToAsync(object parameter)
    {
        await StopOutgoingTypingAsync();
        ClearOtherUserTyping();

        var chatPreviewViewModel = parameter switch
        {
            ChatPreviewViewModel chat => chat,
            Guid selectedChatId => Chats.FirstOrDefault(chat => chat.DirectChatId == selectedChatId)
                                   ?? throw new ArgumentException(
                                       $"No chat found with ID {selectedChatId}.",
                                       nameof(parameter)),
            _ => throw new ArgumentException(
                $"Expected a {nameof(ChatPreviewViewModel)} or chat ID navigation parameter.",
                nameof(parameter))
        };

        CurrentChat?.IsSelected = false;
        CurrentChat = chatPreviewViewModel;
        CurrentChat.IsSelected = true;
        UpdateOutgoingTypingStatus();

        Messages.Clear();
        _nextBeforeSentAt = null;
        _nextBeforeMessageId = null;
        HasOlderMessages = false;
        NotifyMessagesChanged();

        var version = ++_conversationVersion;
        if (CurrentChat.DirectChatId is { } chatId)
        {
            await LoadMessagesAsync(chatId, version, CancellationToken.None);
            if (version == _conversationVersion && CurrentChat?.DirectChatId == chatId)
            {
                await MarkChatReadAsync(chatId);
            }
        }
    }

    public async Task MarkChatReadAsync(Guid chatId)
    {
        var result = await chatWebService.MarkChatReadAsync(chatId);
        if (result.IsFailure)
        {
            LogMarkChatReadFailed(chatId, result.Error.Detail);
            return;
        }

        if (CurrentChat?.DirectChatId == chatId)
        {
            CurrentChat.UnreadMessageCount = 0;
        }
    }

    public async Task OnNavigatedFromAsync()
    {
        await ClearCurrentChatAsync();
    }

    partial void OnCurrentChatChanged(ChatPreviewViewModel? value)
    {
        OnPropertyChanged(nameof(HasCurrentChat));
        OnPropertyChanged(nameof(IsEmptyChat));
        SendMessageCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsLoadingMessagesChanged(bool value)
    {
        OnPropertyChanged(nameof(IsEmptyChat));
    }

    partial void OnDraftMessageChanged(string value)
    {
        UpdateOutgoingTypingStatus();
    }

    private bool CanSendMessage()
    {
        return CurrentChat?.DirectChatId is not null && !string.IsNullOrWhiteSpace(DraftMessage);
    }

    [RelayCommand(CanExecute = nameof(CanSendMessage))]
    private async Task SendMessageAsync()
    {
        var chatId = CurrentChat?.DirectChatId;
        if (chatId is null)
        {
            return;
        }

        var content = DraftMessage.Trim();
        var result = await messageWebService.SendMessageAsync(
            new SendMessageRequest(chatId.Value, content));

        if (result.IsFailure)
        {
            toastService.ShowError(result.Error);
            return;
        }

        if (CurrentChat?.DirectChatId == result.Value.ChatId)
        {
            AddMessage(new GetMessagesItem(
                result.Value.MessageId,
                result.Value.ChatId,
                result.Value.SenderUserId,
                result.Value.Content,
                result.Value.SentAt));
        }

        await StopOutgoingTypingAsync();
        DraftMessage = string.Empty;
    }

    public async Task CloseChatAsync(Guid chatId)
    {
        if (CurrentChat?.DirectChatId == chatId)
        {
            await ClearCurrentChatAsync();
        }
    }

    private async Task ClearCurrentChatAsync()
    {
        await StopOutgoingTypingAsync();
        ClearOtherUserTyping();
        CurrentChat?.IsSelected = false;
        CurrentChat = null;
        Messages.Clear();
        HasOlderMessages = false;
        _conversationVersion++;
        NotifyMessagesChanged();
    }

    [LoggerMessage(LogLevel.Warning, "Failed to mark chat {ChatId} as read: {ErrorDetail}")]
    private partial void LogMarkChatReadFailed(Guid chatId, string errorDetail);
}
