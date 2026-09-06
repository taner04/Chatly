namespace Chatly.WebApi.Features.Health.Endpoints.CheckStatus;

internal sealed class CheckStatusEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/health/status",
                async (
                    ChatlyDbContext dbContext,
                    AzureBlobService blobService,
                    CancellationToken cancellationToken) =>
                {
                    if (!await dbContext.Database.CanConnectAsync(cancellationToken) ||
                        !await blobService.IsReadyAsync(cancellationToken))
                    {
                        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
                    }

                    return Results.Ok();
                })
            .WithName("CheckStatus")
            .WithTags("Health")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status503ServiceUnavailable);
    }
}