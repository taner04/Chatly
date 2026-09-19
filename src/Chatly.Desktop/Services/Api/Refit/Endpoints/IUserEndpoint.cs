using Chatly.Contracts.Common.Pagination;
using Chatly.Contracts.Features.Users.Endpoints.GetCurrentUser;
using Chatly.Contracts.Features.Users.Endpoints.SearchUsers;
using Chatly.Contracts.Features.Users.Endpoints.UpdateUsername;
using Refit;

namespace Chatly.Desktop.Services.Api.Refit.Endpoints;

public interface IUserEndpoint
{
    [Get(ApiRoutes.Users.Search)]
    Task<ApiResponse<PaginationResult<UserSearchResponse>>> SearchUsersAsync(
        [AliasAs("searchName")] string searchName,
        [AliasAs("pageIndex")] int pageIndex,
        [AliasAs("pageSize")] int pageSize,
        CancellationToken cancellationToken);

    [Get(ApiRoutes.Users.Current)]
    Task<ApiResponse<CurrentUserResponse>> GetCurrentUserAsync(
        CancellationToken cancellationToken);

    [Put(ApiRoutes.Users.Username)]
    Task<ApiResponse<CurrentUserResponse>> UpdateUsernameAsync(
        [Body] UpdateUsernameRequest request,
        CancellationToken cancellationToken);

    [Multipart]
    [Put(ApiRoutes.Users.ProfilePicture)]
    Task<ApiResponse<CurrentUserResponse>> UpdateProfilePictureAsync(
        [AliasAs("file")] StreamPart? file,
        CancellationToken cancellationToken);

    [Multipart]
    [Put(ApiRoutes.Users.Onboarding)]
    Task<ApiResponse<CurrentUserResponse>> CompleteOnboardingAsync(
        [AliasAs("newUsername")] string newUsername,
        [AliasAs("file")] StreamPart? file,
        CancellationToken cancellationToken);
}