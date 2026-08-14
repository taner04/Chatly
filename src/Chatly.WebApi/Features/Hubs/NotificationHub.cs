using Chatly.Contracts.SignalR;
using Chatly.WebApi.Common.Infrastructure;
using Chatly.WebApi.Features.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Features.Hubs;


public static class HubGroup
{
    public static string User(UserId userId) => $"user:{userId.Value}";
}

[Authorize]
public sealed class NotificationHub(CurrentUserService currentUser) : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        var userId = currentUser.GetCurrentUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, HubGroup.User(userId));

        await base.OnConnectedAsync();
    }
}