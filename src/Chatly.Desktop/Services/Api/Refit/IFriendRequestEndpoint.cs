using System.Threading;
using System.Threading.Tasks;
using Chatly.Contracts.FriendRequests.Requests;
using Refit;

namespace Chatly.Desktop.Services.Api.Refit;

public interface IFriendRequestEndpoint
{
    [Post("/api/friend-requests")]
    Task<IApiResponse> SendFriendRequestAsync(
        [Body] SendFriendRequestRequest request,
        CancellationToken cancellationToken);
}
