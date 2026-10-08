using Chatly.Contracts.Features.Hubs;
using Chatly.WebApi.Features.DeviceSessions.Exceptions;
using Chatly.WebApi.Features.DeviceSessions.Models;
using Chatly.WebApi.Features.DeviceSessions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chatly.WebApi.Features.Hubs;

[Authorize]
public abstract class HubBase<THubClient> : Hub<THubClient> where THubClient : class, IHubClient
{
    private readonly DeviceSessionService _deviceSessionService;

    private protected HubBase(ChatlyDbContext context, DeviceSessionService deviceSessionService)
    {
        Database = context;
        _deviceSessionService = deviceSessionService;
    }

    protected ChatlyDbContext Database { get; }

    protected UserId CurrentUserId =>
        CurrentUserService.FindUserId(Context.User) ?? throw new HubException("The user is not authenticated.");

    private protected Task<List<UserId>> GetFriendUserIdsAsync(UserId userId, CancellationToken cancellationToken) =>
        Database.Friendships
            .AsNoTracking()
            .ForUser(userId)
            .SelectOtherUserId(userId)
            .ToListAsync(cancellationToken);

    public override async Task OnConnectedAsync()
    {
        var userId = CurrentUserId;
        var headers = Context.GetHttpContext()?.Request.Headers
                      ?? throw new HubException("The device session headers are missing.");
        DeviceSession session;
        try
        {
            session = await _deviceSessionService.GetOrCreateActiveAsync(
                userId,
                DeviceInfoReader.Read(headers),
                CurrentUserService.FindIdentitySessionId(Context.User),
                Context.ConnectionAborted);
        }
        catch (DeviceSessionRevokedException)
        {
            throw new HubException("This device was signed out.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, HubGroups.User(userId));
        await Groups.AddToGroupAsync(Context.ConnectionId, HubGroups.DeviceSession(session.Id));
        await base.OnConnectedAsync();
    }

    protected UserId? GetConnectedUserId() => CurrentUserService.FindUserId(Context.User);
}