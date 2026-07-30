using Chatly.Contracts;
using Chatly.WebApi.Common.Infrastructure;
using Chatly.WebApi.Common.Infrastructure.Persistence;
using Chatly.WebApi.Features.Chats.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chatly.WebApi.Features.Chats.Hubs;

[Authorize]
internal sealed class ChatHub(
    ChatlyDbContext dbContext,
    UserContext userContext) : Hub<IChatClient>
{
    public override async Task OnConnectedAsync()
    {
        var cancellationToken = Context.ConnectionAborted;

        var chatIds = await dbContext.ChatMembers
            .AsNoTracking()
            .Where(member => member.UserId == userContext.UserId)
            .Select(member => member.ChatId)
            .ToListAsync(cancellationToken);

        foreach (var chatId in chatIds)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                GetGroupName(chatId),
                cancellationToken);
        }

        await base.OnConnectedAsync();
    }

    private static string GetGroupName(ChatId chatId) => $"chat:{chatId.Value:N}";
}
