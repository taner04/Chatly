namespace Chatly.Contracts.Endpoints.Users.Requests;

public sealed record UpdateProfilePictureRequest(
    Stream? Content = null,
    string? FileName = null,
    string? ContentType = null);
