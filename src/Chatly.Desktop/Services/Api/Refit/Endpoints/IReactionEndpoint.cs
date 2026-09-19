using Chatly.Contracts.Features.Reactions.Endpoints.SetReaction;
using Chatly.Contracts.Features.Reactions.Models;
using Refit;

namespace Chatly.Desktop.Services.Api.Refit.Endpoints;

public interface IReactionEndpoint
{
    [Put(ApiRoutes.Messages.Reaction)]
    Task<ApiResponse<MessageReactionContract>> SetReactionAsync(
        Guid messageId,
        [Body] SetReactionRequest request,
        CancellationToken cancellationToken);

    [Delete(ApiRoutes.Reactions.ById)]
    Task<IApiResponse> RemoveReactionAsync(
        Guid reactionId,
        CancellationToken cancellationToken);
}