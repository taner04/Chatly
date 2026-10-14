using System.ComponentModel.DataAnnotations;
using Chatly.Shared.Attributes;

namespace Chatly.WebApi.Common.Composition.Options;

[Option]
public sealed class RateLimitingOption
{
    [Range(1, int.MaxValue, ErrorMessage = "HTTP permit limit must be at least 1.")]
    public int HttpPermitLimit { get; init; }

    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00", ErrorMessage = "HTTP window must be between 1 second and 1 day.")]
    public TimeSpan HttpWindow { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Hub permit limit must be at least 1.")]
    public int HubPermitLimit { get; init; }

    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00", ErrorMessage = "Hub window must be between 1 second and 1 day.")]
    public TimeSpan HubWindow { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Hub connection permit limit must be at least 1.")]
    public int HubConnectionPermitLimit { get; init; }

    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00", ErrorMessage = "Hub connection window must be between 1 second and 1 day.")]
    public TimeSpan HubConnectionWindow { get; init; }
}
