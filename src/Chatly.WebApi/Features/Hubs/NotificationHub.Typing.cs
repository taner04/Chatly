using Chatly.Contracts.Features.Hubs.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Features.Hubs;

public sealed partial class NotificationHub
{
    public Task StartTyping(Guid chatId) => PublishTypingStatusAsync(chatId, true);

    public Task StopTyping(Guid chatId) => PublishTypingStatusAsync(chatId, false);

    private async Task PublishTypingStatusAsync(Guid chatId, bool isTyping)
    {
        var userId = await GetCurrentUserIdAsync(Context.ConnectionAborted);
        var chat = await chatAccessService.GetAsync(
                       ChatId.From(chatId),
                       userId,
                       Context.ConnectionAborted)
                   ?? throw new HubException("Chat was not found.");

        await Clients.Group(NotificationHubGroups.User(chat.OtherParticipantUserId))
            .Receive(new TypingStatusChangedNotification(chatId, isTyping));
    }
}