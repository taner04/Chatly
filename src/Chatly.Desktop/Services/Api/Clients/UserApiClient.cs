using Chatly.Contracts.Common.Pagination;
using Chatly.Contracts.Features.Users.Endpoints.CompleteOnboarding;
using Chatly.Contracts.Features.Users.Endpoints.GetCurrentUser;
using Chatly.Contracts.Features.Users.Endpoints.SearchUsers;
using Chatly.Contracts.Features.Users.Endpoints.UpdateProfilePicture;
using Chatly.Contracts.Features.Users.Endpoints.UpdateUsername;
using Chatly.Desktop.Services.Api.Multipart;
using Chatly.Desktop.Services.Api.Refit;
using Chatly.Desktop.Services.Api.Results;

namespace Chatly.Desktop.Services.Api.Clients;

[TransientService]
public sealed class UserApiClient(IChatlyApi chatlyApi)
{
    internal async Task<WebClientResult<PaginationResult<UserSearchResponse>>> SearchUsersAsync(
        SearchUsersRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.SearchUsersAsync(
                request.SearchName,
                request.PageIndex,
                request.PageSize,
                cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult<CurrentUserResponse>> GetCurrentUserAsync(
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.GetCurrentUserAsync(cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult<CurrentUserResponse>> UpdateUsernameAsync(
        UpdateUsernameRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.UpdateUsernameAsync(request, cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult<CurrentUserResponse>> UpdateProfilePictureAsync(
        UpdateProfilePictureRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var file = MultipartStreamPartFactory.CreateBorrowed(
            request.Content,
            request.FileName,
            request.ContentType,
            nameof(request));

        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.UpdateProfilePictureAsync(file, cancellationToken),
            cancellationToken);
    }

    internal async Task<WebClientResult<CurrentUserResponse>> CompleteOnboardingAsync(
        CompleteOnboardingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var file = MultipartStreamPartFactory.CreateBorrowed(
            request.Content,
            request.FileName,
            request.ContentType,
            nameof(request));

        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.CompleteOnboardingAsync(
                request.NewUsername,
                file,
                cancellationToken),
            cancellationToken);
    }
}