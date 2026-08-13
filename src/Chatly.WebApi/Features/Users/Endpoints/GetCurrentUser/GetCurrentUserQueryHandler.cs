using Chatly.Contracts.Users.Results;
using Chatly.WebApi.Features.Users.Services;
using Mediator;

namespace Chatly.WebApi.Features.Users.Endpoints.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(UserService userService)
    : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public async ValueTask<CurrentUserResponse> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUserAsync(cancellationToken);
        return userService.CreateResponse(user);
    }
}
