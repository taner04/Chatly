using Chatly.Contracts.Endpoints.FriendRequests.Requests;

namespace Chatly.WebApi.Features.FriendRequests.Endpoints.SendFriendRequest;

public sealed class SendFriendRequestEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(
                "/api/friend-requests",
                async (
                    [FromBody] SendFriendRequestRequest request,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var command = new SendFriendRequestCommand(
                        UserId.From(request.ReceiverId));

                    await mediator.Send(command, cancellationToken);
                    return Results.NoContent();
                })
            .WithName("SendFriendRequest")
            .WithTags("Friend Request")
            .RequireAuthorization()
            .Accepts<SendFriendRequestRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesStandardErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status409Conflict);
    }
}