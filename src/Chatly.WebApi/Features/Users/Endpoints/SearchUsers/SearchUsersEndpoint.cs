using Chatly.Contracts.Common.Pagination;

namespace Chatly.WebApi.Features.Users.Endpoints.SearchUsers;

internal sealed class SearchUsersEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(
                ApiRoutes.Users.Search,
                async (
                    [AsParameters] SearchUsersRequest request,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var query = new SearchUsersQuery(
                        request.SearchName,
                        request.PageIndex,
                        request.PageSize);

                    return Results.Ok(
                        await mediator.Send(query, cancellationToken));
                })
            .WithName("SearchUsers")
            .WithTags("User")
            .RequireAuthorization()
            .Produces<PaginationResult<UserSearchResponse>>()
            .ProducesStandardErrors(StatusCodes.Status400BadRequest);
    }
}