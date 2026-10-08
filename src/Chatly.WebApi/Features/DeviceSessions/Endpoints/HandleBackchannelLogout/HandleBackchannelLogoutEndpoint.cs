using Chatly.WebApi.Features.DeviceSessions.Services;

namespace Chatly.WebApi.Features.DeviceSessions.Endpoints.HandleBackchannelLogout;

internal sealed class HandleBackchannelLogoutEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(
                ApiRoutes.Identity.BackchannelLogout,
                async (
                    [FromForm(Name = "logout_token")] string? logoutToken,
                    HttpResponse response,
                    [FromServices] BackchannelLogoutTokenValidator validator,
                    [FromServices] DeviceSessionService deviceSessionService,
                    [FromServices] ChatlyDbContext context,
                    CancellationToken cancellationToken) =>
                {
                    response.Headers.CacheControl = "no-store";

                    var identitySessionId = await validator.ValidateAsync(logoutToken, cancellationToken);
                    if (identitySessionId is null)
                    {
                        return Results.BadRequest();
                    }

                    var sessions = await deviceSessionService.RevokeByIdentitySessionAsync(
                        identitySessionId,
                        cancellationToken);
                    if (sessions.Count == 0)
                    {
                        return Results.Ok();
                    }

                    await context.SaveChangesAsync(cancellationToken);
                    await deviceSessionService.PublishRevokedAsync(sessions);

                    return Results.Ok();
                })
            .WithName("HandleBackchannelLogout")
            .WithTags("DeviceSessions")
            .AllowAnonymous()
            .DisableAntiforgery()
            .ExcludeFromDescription();
    }
}