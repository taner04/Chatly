using Chatly.WebApi.Features.Users.Services;

namespace Chatly.WebApi.Features.Users.Endpoints.GetCurrentUser;

internal sealed class GetCurrentUserQueryHandler(UserService userService)
    : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public async ValueTask<CurrentUserResponse> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUserAsync(cancellationToken);
        return userService.CreateResponse(user);
    }
}
