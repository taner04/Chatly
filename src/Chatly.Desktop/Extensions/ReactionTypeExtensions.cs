using Chatly.Contracts.Features.Reactions.Models;

namespace Chatly.Desktop.Extensions;

internal static class ReactionTypeExtensions
{
    extension(ReactionType reactionType)
    {
        public string ToEmoji()
        {
            return reactionType switch
            {
                ReactionType.Like => "👍",
                ReactionType.Love => "❤️",
                ReactionType.Laugh => "😂",
                ReactionType.Surprised => "😮",
                ReactionType.Sad => "😢",
                ReactionType.Angry => "😡",
                ReactionType.Dislike => "👎",
                ReactionType.Smile => "😊",
                ReactionType.Fire => "🔥",
                ReactionType.Celebrate => "🎉",
                ReactionType.Clap => "👏",
                ReactionType.Thanks => "🙏",
                _ => throw new ArgumentOutOfRangeException(nameof(reactionType), reactionType, null)
            };
        }
    }
}