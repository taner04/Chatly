using Chatly.Contracts.SignalR;
using Chatly.WebApi.Features.Chats.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Features.Hubs;

public sealed partial class NotificationHub
{
    public Task StartTyping(Guid chatId)
    {
        return PublishTypingStatusAsync(chatId, true);
    }

    public Task StopTyping(Guid chatId)
    {
        return PublishTypingStatusAsync(chatId, false);
    }

    private async Task PublishTypingStatusAsync(Guid chatId, bool isTyping)
    {
        var userId = await GetCurrentUserIdAsync(Context.ConnectionAborted);
        var chat = await context.Chats
                       .AsNoTracking()
                       .Where(chat => chat.Id == ChatId.From(chatId))
                       .Where(chat => context.Friendships.Any(friendship =>
                           friendship.FirstUserId == chat.FirstUserId &&
                           friendship.SecondUserId == chat.SecondUserId))
                       .Select(chat => new
                       {
                           chat.FirstUserId,
                           chat.SecondUserId
                       })
                       .FirstOrDefaultAsync(Context.ConnectionAborted)
                   ?? throw new HubException("Chat was not found.");

        var receiverUserId = chat.FirstUserId == userId
            ? chat.SecondUserId
            : chat.SecondUserId == userId
                ? chat.FirstUserId
                : throw new HubException("Chat was not found.");

        await Clients.Group(NotificationHubGroups.User(receiverUserId))
            .Receive(new TypingStatusChangedMessage(chatId, isTyping));
    }
}