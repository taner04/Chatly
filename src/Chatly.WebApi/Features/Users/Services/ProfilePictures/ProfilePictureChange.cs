namespace Chatly.WebApi.Features.Users.Services.ProfilePictures;

internal readonly record struct ProfilePictureChange(
    string? NewBlobName,
    string? PreviousBlobName);