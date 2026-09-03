namespace Chatly.WebApi.Features.Users.Endpoints.UpdateProfilePicture;

public sealed record UpdateProfilePictureCommand(
    Stream Content,
    string FileName,
    string ContentType,
    long Length) : ICommand<CurrentUserResponse>;