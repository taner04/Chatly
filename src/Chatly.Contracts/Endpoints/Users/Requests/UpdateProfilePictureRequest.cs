namespace Chatly.Contracts.Endpoints.Users.Requests;

public sealed record UpdateProfilePictureRequest(
    Stream Content,
    string FileName,
    string ContentType);