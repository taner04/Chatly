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
}