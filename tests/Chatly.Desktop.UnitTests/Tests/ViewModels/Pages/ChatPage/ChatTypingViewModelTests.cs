using Chatly.Contracts.Features.Hubs.Abstraction;
using Chatly.Contracts.Features.Hubs.Notifications.NotificationHubServer;
using Chatly.Desktop.UnitTests.Infrastructure;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chatly.Desktop.UnitTests.Tests.ViewModels.Pages.ChatPage;

public sealed class ChatTypingViewModelTests
{
    private readonly Guid _chatId = Guid.NewGuid();
    private readonly INotificationHubServer _hub = Substitute.For<INotificationHubServer>();

    [Fact]
    public Task CurrentDraftChanged_Should_SendStartTypingOnce_When_UserKeepsTyping() => UiThread.RunAsync(async () =>
    {
        var viewModel = await CreateAsync();

        viewModel.CurrentDraftChanged("h");
        viewModel.CurrentDraftChanged("he");
        viewModel.CurrentDraftChanged("hey");

        await _hub.Received(1).StartTyping(_chatId);
    });

    [Fact]
    public Task CurrentDraftChanged_Should_SendStopTyping_When_DraftIsCleared() => UiThread.RunAsync(async () =>
    {
        var viewModel = await CreateAsync();
        viewModel.CurrentDraftChanged("hey");

        viewModel.CurrentDraftChanged(string.Empty);

        await WaitUntilAsync(() =>
            _hub.ReceivedCalls().Any(call => call.GetMethodInfo().Name == nameof(INotificationHubServer.StopTyping)));
        await _hub.Received(1).StopTyping(_chatId);
    });

    [Fact]
    public Task CurrentDraftChanged_Should_StopTypingAutomatically_When_UserIsIdle() => UiThread.RunAsync(async () =>
    {
        var viewModel = await CreateAsync();

        viewModel.CurrentDraftChanged("hey");

        await WaitUntilAsync(
            () => _hub.ReceivedCalls()
                .Any(call => call.GetMethodInfo().Name == nameof(INotificationHubServer.StopTyping)),
            TimeSpan.FromSeconds(5));
        await _hub.Received(1).StopTyping(_chatId);
    });

    [Fact]
    public Task ReceiveTypingStatus_Should_ShowIndicatorOnlyForOpenChat_When_StatusArrives() =>
        UiThread.RunAsync(async () =>
        {
            var viewModel = await CreateAsync();

            viewModel.ReceiveTypingStatus(new TypingStatusChangedNotification(Guid.NewGuid(), true));
            var fromOtherChat = viewModel.IsOtherUserTyping;
            viewModel.ReceiveTypingStatus(new TypingStatusChangedNotification(_chatId, true));

            fromOtherChat.Should().BeFalse();
            viewModel.IsOtherUserTyping.Should().BeTrue();
        });

    private async Task<ChatTypingViewModel> CreateAsync()
    {
        var viewModel = new ChatTypingViewModel(_hub, NullLogger<ChatTypingViewModel>.Instance);
        await viewModel.CurrentChatChangedAsync(_chatId);
        return viewModel;
    }

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(2));
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(50);
        }
    }
}