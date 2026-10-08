using Chatly.WebApi.Common.Infrastructure.Email;
using Chatly.WebApi.Features.Users.Exceptions;
using Chatly.WebApi.Features.Users.Services;
using Chatly.WebApi.Features.Users.Services.ProfilePictures;

namespace Chatly.WebApi.Features.Users.Endpoints.CompleteOnboarding;

internal sealed partial class CompleteOnboardingCommandHandler(
    IEmailService emailService,
    UserService userService,
    ProfilePictureService profilePictureService,
    ILogger<CompleteOnboardingCommandHandler> logger)
    : ICommandHandler<CompleteOnboardingCommand, CurrentUserResponse>
{
    public async ValueTask<CurrentUserResponse> Handle(
        CompleteOnboardingCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUserAsync(cancellationToken);
        if (user.OnboardingCompleted)
        {
            throw new OnboardingAlreadyCompletedException();
        }

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
            await userService.SaveAsync(user, cancellationToken);
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

        try
        {
            await emailService.SendEmailAsync(user.Email, emailRequest, cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            LogWelcomeEmailFailed(exception);
        }

        return userService.CreateResponse(user);
    }

    [LoggerMessage(LogLevel.Warning, "The welcome email could not be sent.")]
    private partial void LogWelcomeEmailFailed(Exception exception);
}