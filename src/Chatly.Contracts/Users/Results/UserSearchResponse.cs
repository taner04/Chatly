namespace Chatly.Contracts.Users.Results;

public sealed record UserSearchResponse(Guid UserId, string Username, Uri? ProfilePictureUrl);
