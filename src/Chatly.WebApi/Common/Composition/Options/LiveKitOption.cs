using System.ComponentModel.DataAnnotations;
using Chatly.Shared.Attributes;

namespace Chatly.WebApi.Common.Composition.Options;

[Option]
public sealed class LiveKitOption
{
    [Required(ErrorMessage = "LiveKit server URL is required.")]
    public string ServerUrl { get; init; } = null!;

    [Required(ErrorMessage = "LiveKit API key is required.")]
    public string ApiKey { get; init; } = null!;

    [Required(ErrorMessage = "LiveKit API secret is required.")]
    [MinLength(32, ErrorMessage = "LiveKit API secret must be at least 32 characters.")]
    public string ApiSecret { get; init; } = null!;
}
