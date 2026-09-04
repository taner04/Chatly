using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chatly.Contracts.Endpoints.Friendships.Results;
using Refit;

namespace Chatly.Desktop.Services.Api.Refit;

public interface IFriendshipEndpoint
{
    [Get("/api/friendships")]
    Task<ApiResponse<IReadOnlyList<GetFriendshipsResponse>>> GetFriendshipsAsync(
        CancellationToken cancellationToken);
}
