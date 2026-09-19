using Chatly.Contracts.Features.Reactions.Endpoints.SetReaction;
using Chatly.Contracts.Features.Reactions.Models;
using Chatly.Desktop.Services.Api.Refit;
using Chatly.Desktop.Services.Api.Results;

namespace Chatly.Desktop.Services.Api.Clients;

[TransientService]
public sealed class ReactionApiClient(IChatlyApi chatlyApi)
{
    internal Task<WebClientResult<MessageReactionContract>> SetReactionAsync(
        Guid messageId,
        ReactionType reactionType,
        CancellationToken cancellationToken = default)
    {
        return ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.SetReactionAsync(
                messageId,
                new SetReactionRequest(reactionType),
                cancellationToken),
            cancellationToken);
    }

    internal Task<WebClientResult> RemoveReactionAsync(
        Guid reactionId,
        CancellationToken cancellationToken = default)
    {
        return ApiRequestExecutor.ExecuteAsync(
            () => chatlyApi.RemoveReactionAsync(reactionId, cancellationToken),
            cancellationToken);
    }
}