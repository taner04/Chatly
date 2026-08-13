using Chatly.Contracts.Users.Results;
using Chatly.WebApi.Common.Infrastructure.Persistence;
using Chatly.WebApi.Features.Users.Services;
using Mediator;

namespace Chatly.WebApi.Features.Users.Endpoints.CompleteOnboarding;

public sealed class CompleteOnboardingCommandHandler(
    UserService userService,
    ProfilePictureService profilePictureService,
    ChatlyDbContext context)
    : ICommandHandler<CompleteOnboardingCommand, CurrentUserResponse>
{
    public async ValueTask<CurrentUserResponse> Handle(
        CompleteOnboardingCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUserAsync(cancellationToken);
        await userService.UpdateUsernameAsync(
            user,
            command.NewUsername,
            cancellationToken);

        ProfilePictureService.ProfilePictureChange? pictureChange = null;
        if (command.Content is not null && command.ContentType is not null)
        {
            pictureChange = await profilePictureService.PrepareReplacementAsync(
                user,
                command.Content,
                command.ContentType,
                cancellationToken);
        }

        user.OnboardingCompleted = true;

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (pictureChange is { } change)
            {
                await profilePictureService.RollbackAsync(change);
            }

            throw;
        }

        if (pictureChange is { } completedChange)
        {
            await profilePictureService.CompleteAsync(completedChange, cancellationToken);
        }
        return userService.CreateResponse(user);
    }
}
