using Chatly.WebApi.Features.Users.Services;
using Chatly.WebApi.Features.Users.Services.Profiles;

namespace Chatly.WebApi.Features.Users.Endpoints.UpdateUsername;

internal sealed class UpdateUsernameCommandHandler(
    UserService userService,
    UserProfileUpdateNotifier profileUpdateNotifier)
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

        await userService.SaveAsync(user, cancellationToken);
        var response = userService.CreateResponse(user);
        await profileUpdateNotifier.PublishAsync(response, cancellationToken);

        return response;
    }
}