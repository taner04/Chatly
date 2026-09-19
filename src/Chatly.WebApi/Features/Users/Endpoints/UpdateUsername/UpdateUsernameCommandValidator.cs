namespace Chatly.WebApi.Features.Users.Endpoints.UpdateUsername;

internal sealed class UpdateUsernameCommandValidator : AbstractValidator<UpdateUsernameCommand>
{
    public UpdateUsernameCommandValidator()
    {
        RuleFor(command => command.NewUsername)
            .AddUsernameRules();
    }
}