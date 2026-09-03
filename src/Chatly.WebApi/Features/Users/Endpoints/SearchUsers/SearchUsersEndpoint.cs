using Chatly.Contracts.Pagination;

namespace Chatly.WebApi.Features.Users.Endpoints.SearchUsers;

public sealed class SearchUsersEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(
                "/api/users/search",
                async (
                    [FromQuery] string searchName,
                    [FromQuery] int pageIndex,
                    [FromQuery] int pageSize,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var query = new SearchUsersQuery(
                        searchName,
                        pageIndex,
                        pageSize);

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