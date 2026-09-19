using System.Text.Json.Serialization;

namespace Chatly.Contracts.Features.Reactions.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReactionType
{
    Like,
    Love,
    Laugh,
    Surprised,
    Sad,
    Angry,
    Dislike,
    Smile,
    Fire,
    Celebrate,
    Clap,
    Thanks
}