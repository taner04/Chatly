using Chatly.WebApi.Features.Reactions.Models;

namespace Chatly.WebApi.Features.Reactions.Endpoints.RemoveReaction;

internal sealed record RemoveReactionCommand(ReactionId ReactionId) : ICommand;