using Chatly.Contracts.Features.Reactions.Models;

namespace Chatly.WebApi.Features.Reactions.Endpoints.SetReaction;

internal sealed record SetReactionCommand(MessageId MessageId, ReactionType ReactionType)
    : ICommand<MessageReactionContract>;