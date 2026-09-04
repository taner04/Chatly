using Chatly.Contracts.SignalR;
using Chatly.WebApi.Features.Users.Services;

namespace Chatly.WebApi.Features.Users.Endpoints.UpdateProfilePicture;

public sealed class UpdateProfilePictureCommandHandler(
    UserService userService,
    ProfilePictureService profilePictureService,
    ChatlyDbContext context,
    NotificationPublisher notificationPublisher)
    : ICommandHandler<UpdateProfilePictureCommand, CurrentUserResponse>
{
    public async ValueTask<CurrentUserResponse> Handle(
        UpdateProfilePictureCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUserAsync(cancellationToken);
        ProfilePictureService.ProfilePictureChange pictureChange;

        if (command.File is null)
        {
            pictureChange = profilePictureService.PrepareRemoval(user);
        }
        else
        {
            await using var content = command.File.OpenReadStream();
            pictureChange = await profilePictureService.PrepareReplacementAsync(
                user,
                content,
                command.File.ContentType,
                cancellationToken);
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await profilePictureService.RollbackAsync(pictureChange);
            throw;
        }

        await profilePictureService.CompleteAsync(pictureChange, cancellationToken);
        var response = userService.CreateResponse(user);
        var recipientIds = await userService.GetProfileUpdateRecipientIdsAsync(user.Id, cancellationToken);

        await notificationPublisher.PublishAsync(recipientIds, new UserProfileUpdatedMessage(
            response.UserId,
            response.Username,
            response.ProfilePictureUrl));

        return response;
    }
}
