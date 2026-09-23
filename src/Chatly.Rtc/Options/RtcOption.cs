using System.ComponentModel.DataAnnotations;
using Chatly.Shared.Attributes;

namespace Chatly.Rtc.Options;

[Option]
public sealed class RtcOption
{
    [Required(ErrorMessage = "Ice servers is required.")]
    [MinLength(1, ErrorMessage = "At least one ICE server is required.")]
    public IReadOnlyList<string> IceServers { get; init; } = [];
}
