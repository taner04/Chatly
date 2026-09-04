namespace Chatly.WebApi.Features.Users.Endpoints.UpdateProfilePicture;

public sealed record UpdateProfilePictureCommand(IFormFile? File) : ICommand<CurrentUserResponse>;
