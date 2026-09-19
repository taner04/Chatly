namespace Chatly.Contracts.Features.Users.Endpoints.SearchUsers;

public sealed record UserSearchResponse(
    Guid UserId,
    string Username,
    string? ProfilePictureUrl,
    UserRelationshipStatus RelationshipStatus);