using Chatly.WebApi.Common.Infrastructure.Email;
using Chatly.WebApi.Features.Users.Services;
using Chatly.WebApi.Features.Users.Services.ProfilePictures;

namespace Chatly.WebApi.Features.Users.Endpoints.CompleteOnboarding;

internal sealed class CompleteOnboardingCommandHandler(
    EmailService emailService,
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

        ProfilePictureChange? pictureChange = null;
        if (command.Content is not null &&
            command.FileName is not null &&
            command.ContentType is not null)
        {
            pictureChange = await profilePictureService.PrepareReplacementAsync(
                user,
                command.Content,
                command.FileName,
                command.ContentType,
                command.Length,
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

        var emailRequest = new EmailRequest(
                "Welcome to Chatly!",
                EmailTemplateKeys.Welcome)
            .AddValue("Username", user.Username!);

        await emailService.SendEmailAsync(user.Email, emailRequest, cancellationToken);

        return userService.CreateResponse(user);
    }
}
