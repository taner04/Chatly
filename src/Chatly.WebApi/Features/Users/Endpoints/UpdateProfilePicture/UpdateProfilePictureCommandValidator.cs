using Chatly.WebApi.Features.Users.Validation;
using FluentValidation;

namespace Chatly.WebApi.Features.Users.Endpoints.UpdateProfilePicture;

public sealed class UpdateProfilePictureCommandValidator
    : AbstractValidator<UpdateProfilePictureCommand>
{
    public UpdateProfilePictureCommandValidator()
    {
        When(command => command.File is not null, () =>
        {
            this.AddProfilePictureRules(command => command.File);
        });
    }
}
