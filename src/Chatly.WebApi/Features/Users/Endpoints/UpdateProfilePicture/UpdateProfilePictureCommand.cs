namespace Chatly.WebApi.Features.Users.Endpoints.UpdateProfilePicture;

internal sealed record UpdateProfilePictureCommand(IFormFile? File) : ICommand<CurrentUserResponse>;
