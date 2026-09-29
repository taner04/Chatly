using Chatly.Contracts.Features.Hubs;
using Chatly.Contracts.Features.Hubs.Abstraction;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chatly.WebApi.IntegrationTests.Infrastructure.Api;

public sealed class NotificationHubTestClient(HubConnection connection) : HubTestClient<NotificationMessage>(connection)
{
    public Task StartTypingAsync(Guid chatId) =>
        Connection.InvokeAsync(nameof(INotificationHubServer.StartTyping), chatId);

    public Task StopTypingAsync(Guid chatId) =>
        Connection.InvokeAsync(nameof(INotificationHubServer.StopTyping), chatId);
}