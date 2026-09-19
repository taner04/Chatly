using Chatly.Contracts.Features.Hubs.Notifications;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage;

[SingletonService]
public sealed partial class ChatTypingViewModel(
    INotificationHubServer notificationHubServer,
    ILogger<ChatTypingViewModel> logger) : ObservableObject
{
    private static readonly TimeSpan TypingIdleDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan TypingRefreshInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan TypingStatusExpiry = TimeSpan.FromSeconds(3);
    private Guid? _currentChatId;
    private bool _isTyping;
    private DateTimeOffset? _lastTypingStatusSentAt;
    private CancellationTokenSource? _typingIdleCancellation;
    private CancellationTokenSource? _typingStatusExpiryCancellation;

    [ObservableProperty] public partial bool IsOtherUserTyping { get; private set; }

    internal async Task CurrentChatChangedAsync(Guid? chatId)
    {
        var outgoingChatId = _isTyping ? _currentChatId : null;
        _typingIdleCancellation?.Cancel();
        _typingIdleCancellation = null;
        _isTyping = false;
        _lastTypingStatusSentAt = null;
        _currentChatId = chatId;
        ResetOtherUserTyping();

        if (outgoingChatId is { } previousChatId)
        {
            await SendTypingStatusAsync(previousChatId, false);
        }
    }

    internal void CurrentDraftChanged(string draft)
    {
        if (_currentChatId is not { } chatId)
        {
            return;
        }

        _typingIdleCancellation?.Cancel();
        _typingIdleCancellation = null;

        if (string.IsNullOrWhiteSpace(draft))
        {
            _ = StopOutgoingTypingAsync(chatId);
            return;
        }

        if (!_isTyping)
        {
            _isTyping = true;
            _lastTypingStatusSentAt = DateTimeOffset.UtcNow;
            _ = SendTypingStatusAsync(chatId, true);
        }
        else if (DateTimeOffset.UtcNow - _lastTypingStatusSentAt >= TypingRefreshInterval)
        {
            _lastTypingStatusSentAt = DateTimeOffset.UtcNow;
            _ = SendTypingStatusAsync(chatId, true);
        }

        var cancellation = new CancellationTokenSource();
        _typingIdleCancellation = cancellation;
        _ = StopTypingAfterIdleAsync(chatId, cancellation);
    }

    internal void ReceiveTypingStatus(TypingStatusChangedNotification message)
    {
        if (_currentChatId != message.ChatId)
        {
            return;
        }

        ResetOtherUserTyping();
        IsOtherUserTyping = message.IsTyping;

        if (!message.IsTyping)
        {
            return;
        }

        var cancellation = new CancellationTokenSource();
        _typingStatusExpiryCancellation = cancellation;
        _ = ExpireOtherUserTypingAsync(cancellation);
    }

    internal async Task StopOutgoingTypingAsync(Guid chatId)
    {
        if (_currentChatId != chatId)
        {
            return;
        }

        _typingIdleCancellation?.Cancel();
        _typingIdleCancellation = null;

        if (!_isTyping)
        {
            return;
        }

        _isTyping = false;
        _lastTypingStatusSentAt = null;
        await SendTypingStatusAsync(chatId, false);
    }

    internal void ResetOtherUserTyping()
    {
        _typingStatusExpiryCancellation?.Cancel();
        _typingStatusExpiryCancellation = null;
        IsOtherUserTyping = false;
    }

    private async Task StopTypingAfterIdleAsync(Guid chatId, CancellationTokenSource cancellation)
    {
        try
        {
            await Task.Delay(TypingIdleDelay, cancellation.Token);
            if (ReferenceEquals(_typingIdleCancellation, cancellation) && _currentChatId == chatId)
            {
                _typingIdleCancellation = null;
                _isTyping = false;
                _lastTypingStatusSentAt = null;
                await SendTypingStatusAsync(chatId, false);
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        finally
        {
            cancellation.Dispose();
        }
    }

    private async Task SendTypingStatusAsync(Guid chatId, bool isTyping)
    {
        try
        {
            if (isTyping)
            {
                await notificationHubServer.StartTyping(chatId);
            }
            else
            {
                await notificationHubServer.StopTyping(chatId);
            }
        }
        catch (Exception exception)
        {
            LogTypingStatusFailed(chatId, exception);
        }
    }

    private async Task ExpireOtherUserTypingAsync(CancellationTokenSource cancellation)
    {
        try
        {
            await Task.Delay(TypingStatusExpiry, cancellation.Token);
            UIThreadDispatcher.SafeInvoke(() =>
            {
                if (ReferenceEquals(_typingStatusExpiryCancellation, cancellation))
                {
                    _typingStatusExpiryCancellation = null;
                    IsOtherUserTyping = false;
                }
            });
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        finally
        {
            cancellation.Dispose();
        }
    }

    [LoggerMessage(LogLevel.Debug, "Failed to send typing status for chat {ChatId}.")]
    private partial void LogTypingStatusFailed(Guid chatId, Exception exception);
}