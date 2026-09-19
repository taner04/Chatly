namespace Chatly.WebApi.Features.Users.Services.Profiles;

internal sealed record UserProfileRow(
    UserId UserId,
    string? Username,
    string? ProfilePictureKey);