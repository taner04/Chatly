using Chatly.WebApi.Features.Users.Services;

namespace Chatly.WebApi.Features.Users.Endpoints.UpdateProfilePicture;

public sealed class UpdateProfilePictureCommandHandler(
    UserService userService,
    ProfilePictureService profilePictureService,
    ChatlyDbContext context)
    : ICommandHandler<UpdateProfilePictureCommand, CurrentUserResponse>
{
    public async ValueTask<CurrentUserResponse> Handle(
        UpdateProfilePictureCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUserAsync(cancellationToken);
        var pictureChange = await profilePictureService.PrepareReplacementAsync(
            user,
            command.Content,
            command.ContentType,
            cancellationToken);

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
        return userService.CreateResponse(user);
    }
}