namespace Chatly.Contracts.Users.Requests;

public sealed record UpdateProfilePictureRequest(
    Stream Content,
    string FileName,
    string ContentType);