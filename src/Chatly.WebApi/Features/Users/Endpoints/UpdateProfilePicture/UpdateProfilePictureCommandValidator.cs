using Chatly.WebApi.Features.Users.Validation;
using FluentValidation;

namespace Chatly.WebApi.Features.Users.Endpoints.UpdateProfilePicture;

public sealed class UpdateProfilePictureCommandValidator
    : AbstractValidator<UpdateProfilePictureCommand>
{
    public UpdateProfilePictureCommandValidator()
    {
        this.AddProfilePictureRules(
            command => command.Content,
            command => command.FileName,
            command => command.ContentType,
            command => command.Length);
    }
}