using Chatly.Contracts.Features.Reactions.Models;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage.Messages;

public sealed class ChatReactionViewModel(
    MessageReactionContract reaction,
    bool isOwnReaction,
    string reactorName)
{
    public Guid ReactionId { get; } = reaction.ReactionId;

    public Guid UserId { get; } = reaction.UserId;

    public ReactionType ReactionType { get; } = reaction.ReactionType;

    public string Emoji => ReactionType.ToEmoji();

    public string Tooltip { get; } = $"{reactorName} reacted with {reaction.ReactionType.ToEmoji()}";

    public bool IsOwnReaction { get; } = isOwnReaction;
}