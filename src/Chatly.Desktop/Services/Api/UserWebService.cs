using Chatly.Contracts.Endpoints.Users.Requests;
using Chatly.Contracts.Endpoints.Users.Results;
using Chatly.Contracts.Pagination;
using Chatly.Desktop.Services.Api.Refit.Abstraction;
using Chatly.Desktop.Services.Api.Results;
using Refit;

namespace Chatly.Desktop.Services.Api;

[TransientService]
public sealed class UserWebService(IChatlyApi chatlyApi)
{
    public async Task<WebClientResult<PaginationResult<UserSearchResponse>>> SearchUsersAsync(
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

    public async Task<WebClientResult<CurrentUserResponse>> GetCurrentUserAsync(
        CancellationToken cancellationToken = default)
    {
        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.GetCurrentUserAsync(cancellationToken),
            cancellationToken);
    }

    public async Task<WebClientResult<CurrentUserResponse>> UpdateUsernameAsync(
        UpdateUsernameRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.UpdateUsernameAsync(request, cancellationToken),
            cancellationToken);
    }

    public async Task<WebClientResult<CurrentUserResponse>> UpdateProfilePictureAsync(
        UpdateProfilePictureRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var file = request.Content is null
            ? null
            : new StreamPart(
                request.Content,
                request.FileName
                ?? throw new ArgumentException(
                    "A file name is required when profile-picture content is provided.",
                    nameof(request)),
                request.ContentType);

        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.UpdateProfilePictureAsync(file, cancellationToken),
            cancellationToken);
    }

    public async Task<WebClientResult<CurrentUserResponse>> CompleteOnboardingAsync(
        CompleteOnboardingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var file = request.Content is null
            ? null
            : new StreamPart(
                request.Content,
                request.FileName
                ?? throw new ArgumentException(
                    "A file name is required when profile-picture content is provided.",
                    nameof(request)),
                request.ContentType);

        return await ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.CompleteOnboardingAsync(
                request.NewUsername,
                file,
                cancellationToken),
            cancellationToken);
    }
}
