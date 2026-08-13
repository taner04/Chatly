using Chatly.Contracts.Users.Results;
using Chatly.WebApi.Common.Infrastructure.Persistence;
using Chatly.WebApi.Features.Users.Services;
using Mediator;

namespace Chatly.WebApi.Features.Users.Endpoints.UpdateUsername;

public sealed class UpdateUsernameCommandHandler(
    UserService userService,
    ChatlyDbContext context)
    : ICommandHandler<UpdateUsernameCommand, CurrentUserResponse>
{
    public async ValueTask<CurrentUserResponse> Handle(
        UpdateUsernameCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUserAsync(cancellationToken);
        await userService.UpdateUsernameAsync(
            user,
            command.NewUsername,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return userService.CreateResponse(user);
    }
}
