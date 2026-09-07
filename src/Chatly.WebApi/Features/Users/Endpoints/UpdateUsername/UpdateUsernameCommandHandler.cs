using Chatly.Contracts.SignalR;
using Chatly.WebApi.Features.Users.Services;

namespace Chatly.WebApi.Features.Users.Endpoints.UpdateUsername;

public sealed class UpdateUsernameCommandHandler(
    UserService userService,
    ChatlyDbContext context,
    NotificationPublisher notificationPublisher)
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
        var response = userService.CreateResponse(user);
        var recipientIds = await userService.GetProfileUpdateRecipientIdsAsync(user.Id, cancellationToken);

        await notificationPublisher.PublishAsync(recipientIds, new UserProfileUpdatedMessage(
            response.UserId,
            response.Username,
            response.ProfilePictureUrl));

        return response;
    }
}