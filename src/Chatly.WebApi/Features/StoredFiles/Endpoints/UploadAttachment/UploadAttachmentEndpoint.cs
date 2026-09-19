using Chatly.Contracts.Features.StoredFiles.Endpoints.UploadAttachment;

namespace Chatly.WebApi.Features.StoredFiles.Endpoints.UploadAttachment;

internal sealed class UploadAttachmentEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(
                ApiRoutes.Attachments.Collection,
                async (
                    [FromForm] IFormFile? file,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    return Results.Ok(await mediator.Send(
                        new UploadAttachmentCommand(file),
                        cancellationToken));
                })
            .WithName("UploadAttachment")
            .WithTags("Attachments")
            .RequireAuthorization()
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<UploadAttachmentResponse>()
            .ProducesStandardErrors(StatusCodes.Status400BadRequest);
    }
}