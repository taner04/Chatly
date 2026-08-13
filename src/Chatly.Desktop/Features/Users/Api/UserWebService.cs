using System;
using System.Threading;
using System.Threading.Tasks;
using Chatly.Contracts.Users.Requests;
using Chatly.Contracts.Users.Results;
using Chatly.Desktop.Common.Api;
using Chatly.Desktop.Common.Api.Results;
using Refit;

namespace Chatly.Desktop.Features.Users.Api;

public sealed class UserWebService(IUserEndpoint userEndpoint)
{
    public Task<WebClientResult<CurrentUserResponse>> GetCurrentUserAsync(
        CancellationToken cancellationToken = default) =>
        HttpOrchestrator.ExecuteAsync(
            () => userEndpoint.GetCurrentUserAsync(cancellationToken),
            cancellationToken);

    public Task<WebClientResult<CurrentUserResponse>> UpdateUsernameAsync(
        UpdateUsernameRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HttpOrchestrator.ExecuteAsync(
            () => userEndpoint.UpdateUsernameAsync(request, cancellationToken),
            cancellationToken);
    }

    public async Task<WebClientResult<CurrentUserResponse>> UpdateProfilePictureAsync(
        UpdateProfilePictureRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Content);

        var file = new StreamPart(
            request.Content,
            request.FileName,
            request.ContentType);

        return await HttpOrchestrator.ExecuteAsync(
            () => userEndpoint.UpdateProfilePictureAsync(file, cancellationToken),
            cancellationToken);
    }

    public Task<WebClientResult<GetPictureResponse>> GetCurrentUserProfilePictureAsync(
        CancellationToken cancellationToken = default) =>
        HttpOrchestrator.ExecuteAsync(
            () => userEndpoint.GetCurrentUserProfilePictureAsync(cancellationToken),
            cancellationToken);

    public async Task<WebClientResult<CurrentUserResponse>> CompleteOnboardingAsync(
        CompleteOnboardingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        StreamPart? file = request.Content is null
            ? null
            : new StreamPart(
                request.Content,
                request.FileName
                    ?? throw new ArgumentException(
                        "A file name is required when profile-picture content is provided.",
                        nameof(request)),
                request.ContentType);

        return await HttpOrchestrator.ExecuteAsync(
            () => userEndpoint.CompleteOnboardingAsync(
                request.NewUsername,
                file,
                cancellationToken),
            cancellationToken);
    }
}
