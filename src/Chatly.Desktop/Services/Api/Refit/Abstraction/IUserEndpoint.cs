using Chatly.Contracts.Endpoints.Users.Requests;
using Chatly.Contracts.Endpoints.Users.Results;
using Chatly.Contracts.Pagination;
using Refit;

namespace Chatly.Desktop.Services.Api.Refit.Abstraction;

public interface IUserEndpoint
{
    [Get("/api/users/search")]
    Task<ApiResponse<PaginationResult<UserSearchResponse>>> SearchUsersAsync(
        [AliasAs("searchName")] string searchName,
        [AliasAs("pageIndex")] int pageIndex,
        [AliasAs("pageSize")] int pageSize,
        CancellationToken cancellationToken);

    [Get("/api/users/me")]
    Task<ApiResponse<CurrentUserResponse>> GetCurrentUserAsync(
        CancellationToken cancellationToken);

    [Put("/api/users/me/username")]
    Task<ApiResponse<CurrentUserResponse>> UpdateUsernameAsync(
        [Body] UpdateUsernameRequest request,
        CancellationToken cancellationToken);

    [Multipart]
    [Put("/api/users/me/profile-picture")]
    Task<ApiResponse<CurrentUserResponse>> UpdateProfilePictureAsync(
        [AliasAs("file")] StreamPart? file,
        CancellationToken cancellationToken);

    [Multipart]
    [Put("/api/users/me/onboarding")]
    Task<ApiResponse<CurrentUserResponse>> CompleteOnboardingAsync(
        [AliasAs("newUsername")] string newUsername,
        [AliasAs("file")] StreamPart? file,
        CancellationToken cancellationToken);
}
