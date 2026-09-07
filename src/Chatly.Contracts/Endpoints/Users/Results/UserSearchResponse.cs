namespace Chatly.Contracts.Endpoints.Users.Results;

public sealed record UserSearchResponse(
    Guid UserId,
    string Username,
    Uri? ProfilePictureUrl,
    UserRelationshipStatus RelationshipStatus);