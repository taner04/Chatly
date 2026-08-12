using FluentValidation;

namespace Chatly.WebApi.Features.Users.Endpoints.UpdateUsername;

public class UpdateUsernameCommandValidator : AbstractValidator<UpdateUsernameCommand>
{
    public UpdateUsernameCommandValidator()
    {
        RuleFor(x => x.NewUsername)
            .NotEmpty().WithMessage("Username cannot be empty.")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores.");
    }
}