using Chatly.WebApi.Features.StoredFiles.Models;

namespace Chatly.WebApi.Features.StoredFiles.Endpoints.RemoveAttachment;

internal sealed class RemoveAttachmentEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete(
                ApiRoutes.Attachments.ById,
                async (
                    [FromRoute] Guid attachmentId,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await mediator.Send(
                        new RemoveAttachmentCommand(StoredFileId.From(attachmentId)),
                        cancellationToken);
                    return Results.NoContent();
                })
            .WithName("RemoveAttachment")
            .WithTags("Attachments")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound);
    }
}