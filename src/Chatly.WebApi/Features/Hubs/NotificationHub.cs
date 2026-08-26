using Chatly.Contracts.SignalR;
using Chatly.WebApi.Common.Infrastructure;
using Chatly.WebApi.Common.Infrastructure.Persistence;
using Chatly.WebApi.Features.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chatly.WebApi.Features.Hubs;

public static class HubGroup
{
    public static string User(UserId userId)
    {
        return $"user:{userId.Value}";
    }
}

[Authorize]
public sealed class NotificationHub(
    CurrentUserService currentUser,
    ChatlyDbContext context) : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        var auth0Id = currentUser.GetAuth0Id();
        var userId = await context.Users
            .Where(user => user.Auth0Id == auth0Id)
            .Select(user => user.Id)
            .SingleAsync(Context.ConnectionAborted);

        await Groups.AddToGroupAsync(Context.ConnectionId, HubGroup.User(userId));

        await base.OnConnectedAsync();
    }
}