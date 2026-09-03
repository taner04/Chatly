using Chatly.Contracts.SignalR;
using Chatly.Desktop.Utilities;
using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage;

public sealed partial class ChatPageViewModel
{
    private static readonly TimeSpan TypingIdleDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan TypingRefreshInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan TypingStatusExpiry = TimeSpan.FromSeconds(3);
    private bool _isTyping;
    private DateTimeOffset? _lastTypingStatusSentAt;
    private Guid? _typingChatId;
    private CancellationTokenSource? _typingIdleCancellation;
    private CancellationTokenSource? _typingStatusExpiryCancellation;

    public void ReceiveTypingStatus(TypingStatusChangedMessage message)
    {
        if (CurrentChat?.DirectChatId != message.ChatId)
        {
            return;
        }

        ClearOtherUserTyping();
        IsOtherUserTyping = message.IsTyping;

        if (!message.IsTyping)
        {
            return;
        }

        var cancellation = new CancellationTokenSource();
        _typingStatusExpiryCancellation = cancellation;
        _ = ExpireOtherUserTypingAsync(cancellation);
    }

    private void UpdateOutgoingTypingStatus()
    {
        var chatId = CurrentChat?.DirectChatId;
        if (chatId is null)
        {
            return;
        }

        _typingIdleCancellation?.Cancel();
        _typingIdleCancellation = null;

        if (string.IsNullOrWhiteSpace(DraftMessage))
        {
            _ = StopOutgoingTypingAsync();
            return;
        }

        if (!_isTyping || _typingChatId != chatId)
        {
            _isTyping = true;
            _typingChatId = chatId;
            _lastTypingStatusSentAt = DateTimeOffset.UtcNow;
            _ = SendTypingStatusAsync(chatId.Value, true);
        }
        else if (DateTimeOffset.UtcNow - _lastTypingStatusSentAt >= TypingRefreshInterval)
        {
            _lastTypingStatusSentAt = DateTimeOffset.UtcNow;
            _ = SendTypingStatusAsync(chatId.Value, true);
        }

        var cancellation = new CancellationTokenSource();
        _typingIdleCancellation = cancellation;
        _ = StopTypingAfterIdleAsync(chatId.Value, cancellation);
    }

    private async Task StopTypingAfterIdleAsync(Guid chatId, CancellationTokenSource cancellation)
    {
        try
        {
            await Task.Delay(TypingIdleDelay, cancellation.Token);
            if (ReferenceEquals(_typingIdleCancellation, cancellation) && _typingChatId == chatId)
            {
                _typingIdleCancellation = null;
                _typingChatId = null;
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

    private async Task StopOutgoingTypingAsync()
    {
        _typingIdleCancellation?.CancelAsync();
        _typingIdleCancellation = null;

        if (!_isTyping || _typingChatId is not { } chatId)
        {
            return;
        }

        _isTyping = false;
        _typingChatId = null;
        _lastTypingStatusSentAt = null;
        await SendTypingStatusAsync(chatId, false);
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
            UiThreadDispatcher.SafeInvoke(() =>
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

    private void ClearOtherUserTyping()
    {
        _typingStatusExpiryCancellation?.Cancel();
        _typingStatusExpiryCancellation = null;
        IsOtherUserTyping = false;
    }

    [LoggerMessage(LogLevel.Debug, "Failed to send typing status for chat {ChatId}.")]
    private partial void LogTypingStatusFailed(Guid chatId, Exception exception);
}