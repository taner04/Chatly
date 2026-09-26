using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;
using Chatly.Contracts.Common.Policies;
using Chatly.Contracts.Features.Messages.Endpoints.SendMessage;
using Chatly.Desktop.Abstraction.Storage;
using Chatly.Desktop.Services.Api.Clients;
using Chatly.Desktop.ViewModels.Pages.ChatPage.Messages;
using UserSessionContext = Chatly.Desktop.Models.UserSession.UserSessionContext;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage;

[SingletonService]
public sealed partial class ChatPageViewModel(
    IFilePicker filePicker,
    ChatSidebarViewModel chatSidebar,
    ChatApiClient chatApiClient,
    MessageApiClient messageApiClient,
    ChatMessagesViewModel messagesViewModel,
    ChatTypingViewModel typingViewModel,
    CallViewModel call,
    UserSessionContext userSessionContext,
    IToastService toastService,
    ILogger<ChatPageViewModel> logger)
    : PageViewModelBase
{
    public ObservableCollection<ChatPreviewViewModel> Chats => chatSidebar.Chats;

    public ChatMessagesViewModel MessagesViewModel { get; } = messagesViewModel;

    public ChatTypingViewModel TypingViewModel { get; } = typingViewModel;

    public CallViewModel Call { get; } = call;

    public ObservableCollection<DraftAttachmentViewModel> DraftAttachments { get; } = [];

    [ObservableProperty] public partial ChatPreviewViewModel? CurrentChat { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
    public partial string DraftMessage { get; set; } = string.Empty;

    public bool HasCurrentChat => CurrentChat is not null;

    public bool HasDraftAttachments => DraftAttachments.Count > 0;

    public override async Task OnNavigatedToAsync(
        object? parameter,
        CancellationToken cancellationToken)
    {
        if (parameter is null)
        {
            return;
        }

        if (parameter is not Guid selectedChatId)
        {
            throw new ArgumentException(
                "Expected a chat ID navigation parameter.",
                nameof(parameter));
        }

        var chatPreviewViewModel = Chats.FirstOrDefault(chat => chat.DirectChatId == selectedChatId)
                                   ?? throw new ArgumentException(
                                       $"No chat found with ID {selectedChatId}.",
                                       nameof(parameter));

        var currentUserId = userSessionContext.CurrentUser?.Id
                            ?? throw new InvalidOperationException(
                                "An authenticated user is required to open a conversation.");
        var version = MessagesViewModel.BeginConversation(
            selectedChatId,
            currentUserId,
            chatPreviewViewModel.Name,
            cancellationToken);
        var typingChange = TypingViewModel.CurrentChatChangedAsync(selectedChatId);

        CurrentChat?.IsSelected = false;
        DraftMessage = string.Empty;
        CurrentChat = chatPreviewViewModel;
        CurrentChat.IsSelected = true;
        await typingChange;

        if (MessagesViewModel.IsCurrentConversation(selectedChatId, version))
        {
            await MessagesViewModel.LoadInitialMessagesAsync(version);
            if (MessagesViewModel.IsCurrentConversation(selectedChatId, version))
            {
                await MarkChatReadAsync(selectedChatId);
            }
        }
    }

    public override async Task OnNavigatedFromAsync(CancellationToken cancellationToken)
    {
        await ClearCurrentChatAsync();
    }

    internal async Task MarkChatReadAsync(Guid chatId)
    {
        var result = await chatApiClient.MarkChatReadAsync(chatId);
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

    partial void OnCurrentChatChanged(ChatPreviewViewModel? value)
    {
        OnPropertyChanged(nameof(HasCurrentChat));
        SendMessageCommand.NotifyCanExecuteChanged();
    }

    partial void OnDraftMessageChanged(string value)
    {
        TypingViewModel.CurrentDraftChanged(value);
    }

    private bool CanSendMessage() =>
        CurrentChat?.DirectChatId is not null &&
        (!string.IsNullOrWhiteSpace(DraftMessage) || HasDraftAttachments);

    public async Task AddAttachmentAsync(params IStorageFile[] selectedFiles)
    {
        var remainingSlots = MessageAttachmentPolicy.MaxPerMessage - DraftAttachments.Count;
        if (remainingSlots <= 0)
        {
            toastService.AddNotification(
                $"A message can contain up to {MessageAttachmentPolicy.MaxPerMessage} files.");
            return;
        }

        if (selectedFiles.Length > remainingSlots)
        {
            toastService.AddNotification($"Only {remainingSlots} more file(s) can be attached.");
        }

        foreach (var file in selectedFiles.Take(remainingSlots))
        {
            var size = await GetFileSizeAsync(file);
            switch (size)
            {
                case <= 0:
                    toastService.AddNotification($"{file.Name} is empty.");
                    continue;
                case > MessageAttachmentPolicy.MaxFileSizeBytes:
                    toastService.AddNotification(
                        $"{file.Name} exceeds the {MessageAttachmentPolicy.MaxFileSizeBytes / (1024 * 1024)} MiB file limit.");
                    continue;
            }

            if (DraftAttachments.Sum(attachment => attachment.Size) + size >
                MessageAttachmentPolicy.MaxTotalSizePerMessageBytes)
            {
                toastService.AddNotification(
                    $"Attachments cannot exceed {MessageAttachmentPolicy.MaxTotalSizePerMessageBytes / (1024 * 1024)} MiB in total.");
                continue;
            }

            DraftAttachments.Add(new DraftAttachmentViewModel(file, size));
        }

        NotifyDraftAttachmentsChanged();
    }

    [RelayCommand]
    private async Task AddAttachmentsAsync()
    {
        await AddAttachmentAsync([.. await filePicker.PickFileAsync(true)]);
    }

    [RelayCommand]
    private void RemoveDraftAttachment(DraftAttachmentViewModel? attachment)
    {
        if (attachment is null || !DraftAttachments.Remove(attachment))
        {
            return;
        }

        NotifyDraftAttachmentsChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSendMessage))]
    private async Task SendMessageAsync()
    {
        var chatId = CurrentChat?.DirectChatId;
        if (chatId is null)
        {
            return;
        }

        var conversationVersion = MessagesViewModel.ConversationVersion;
        var submittedDraft = DraftMessage;
        var submittedAttachments = DraftAttachments.ToArray();
        var content = submittedDraft.Trim();
        var result = await messageApiClient.SendMessageAsync(
            new SendMessageRequest(
                chatId.Value,
                string.IsNullOrWhiteSpace(content) ? null : content),
            [.. submittedAttachments.Select(attachment => attachment.File)]);

        if (result.IsFailure)
        {
            if (MessagesViewModel.IsCurrentConversation(chatId.Value, conversationVersion))
            {
                toastService.ShowError(result.Error);
            }

            return;
        }

        MessagesViewModel.AddSentMessage(result.Value, conversationVersion);

        if (!MessagesViewModel.IsCurrentConversation(chatId.Value, conversationVersion) ||
            DraftMessage != submittedDraft ||
            !submittedAttachments.SequenceEqual(DraftAttachments))
        {
            return;
        }

        await TypingViewModel.StopOutgoingTypingAsync(chatId.Value);
        if (MessagesViewModel.IsCurrentConversation(chatId.Value, conversationVersion) &&
            DraftMessage == submittedDraft)
        {
            DraftMessage = string.Empty;
            DraftAttachments.Clear();
            NotifyDraftAttachmentsChanged();
        }
    }

    internal async Task CloseChatAsync(Guid chatId)
    {
        if (CurrentChat?.DirectChatId == chatId)
        {
            await ClearCurrentChatAsync();
        }
    }

    private async Task ClearCurrentChatAsync()
    {
        MessagesViewModel.Reset();
        var typingChange = TypingViewModel.CurrentChatChangedAsync(null);
        CurrentChat?.IsSelected = false;
        DraftMessage = string.Empty;
        DraftAttachments.Clear();
        NotifyDraftAttachmentsChanged();
        CurrentChat = null;
        await typingChange;
    }

    private void NotifyDraftAttachmentsChanged()
    {
        OnPropertyChanged(nameof(HasDraftAttachments));
        SendMessageCommand.NotifyCanExecuteChanged();
    }

    private static async Task<long> GetFileSizeAsync(IStorageFile file)
    {
        var properties = await file.GetBasicPropertiesAsync();
        if (properties.Size is { } size)
        {
            return checked((long)size);
        }

        await using var stream = await file.OpenReadAsync();
        return stream.Length;
    }

    [LoggerMessage(LogLevel.Warning, "Failed to mark chat {ChatId} as read: {ErrorDetail}")]
    private partial void LogMarkChatReadFailed(Guid chatId, string errorDetail);
}