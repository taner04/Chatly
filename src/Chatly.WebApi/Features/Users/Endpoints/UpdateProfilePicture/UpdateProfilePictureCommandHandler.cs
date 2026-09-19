using Chatly.WebApi.Features.Users.Services;
using Chatly.WebApi.Features.Users.Services.ProfilePictures;
using Chatly.WebApi.Features.Users.Services.Profiles;

namespace Chatly.WebApi.Features.Users.Endpoints.UpdateProfilePicture;

internal sealed class UpdateProfilePictureCommandHandler(
    UserService userService,
    ProfilePictureService profilePictureService,
    ChatlyDbContext context,
    UserProfileUpdateNotifier profileUpdateNotifier)
    : ICommandHandler<UpdateProfilePictureCommand, CurrentUserResponse>
{
    public async ValueTask<CurrentUserResponse> Handle(
        UpdateProfilePictureCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUserAsync(cancellationToken);
        ProfilePictureChange pictureChange;

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
                command.File.FileName,
                command.File.ContentType,
                command.File.Length,
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
        await profileUpdateNotifier.PublishAsync(response, cancellationToken);

        return response;
    }
}