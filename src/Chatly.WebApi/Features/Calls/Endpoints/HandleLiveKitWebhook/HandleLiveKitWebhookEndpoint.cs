using Chatly.WebApi.Features.Calls.Jobs;
using Chatly.WebApi.Features.Calls.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace Chatly.WebApi.Features.Calls.Endpoints.HandleLiveKitWebhook;

internal sealed class HandleLiveKitWebhookEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(
                ApiRoutes.LiveKit.Webhook,
                async (
                    HttpRequest request,
                    [FromServices] LiveKitWebhookReceiver receiver,
                    [FromServices] CallExpiryJob callExpiry,
                    CancellationToken cancellationToken) =>
                {
                    using var reader = new StreamReader(request.Body);
                    var body = await reader.ReadToEndAsync(cancellationToken);
                    var webhookEvent = await receiver.ReceiveAsync(body, request.Headers.Authorization);
                    if (webhookEvent is null)
                    {
                        return Results.Unauthorized();
                    }

                    if (webhookEvent.Name == LiveKitWebhookEvent.RoomFinished
                        && Guid.TryParse(webhookEvent.RoomName, out var callId))
                    {
                        await callExpiry.AbandonAsync(callId, cancellationToken);
                    }

                    return Results.Ok();
                })
            .WithName("HandleLiveKitWebhook")
            .WithTags("Calls")
            .AllowAnonymous()
            .DisableAntiforgery()
            // Server-to-server callbacks from a single address; limiting them would drop call and sign-out events.
            .DisableRateLimiting()
            .ExcludeFromDescription();
    }
}