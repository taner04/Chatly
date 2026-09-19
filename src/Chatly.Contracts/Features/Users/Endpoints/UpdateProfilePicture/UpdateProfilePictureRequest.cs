namespace Chatly.Contracts.Features.Users.Endpoints.UpdateProfilePicture;

public sealed record UpdateProfilePictureRequest(
    Stream? Content = null,
    string? FileName = null,
    string? ContentType = null);