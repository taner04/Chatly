using System.Threading;
using System.Threading.Tasks;
using Chatly.Contracts.Users.Requests;
using Chatly.Contracts.Users.Results;
using Refit;

namespace Chatly.Desktop.Features.Users.Api;

public interface IUserEndpoint
{
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
        [AliasAs("file")] StreamPart file,
        CancellationToken cancellationToken);

    [Get("/api/users/me/profile-picture")]
    Task<ApiResponse<GetPictureResponse>> GetCurrentUserProfilePictureAsync(
        CancellationToken cancellationToken);

    [Multipart]
    [Put("/api/users/me/onboarding")]
    Task<ApiResponse<CurrentUserResponse>> CompleteOnboardingAsync(
        [AliasAs("newUsername")] string newUsername,
        [AliasAs("file")] StreamPart? file,
        CancellationToken cancellationToken);
}
