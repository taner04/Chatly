using System.ComponentModel.DataAnnotations;
using Chatly.Shared.Attributes;

namespace Chatly.Desktop.Options;

[Option]
internal sealed class WebApiClientOption
{
    [Required(ErrorMessage = "BaseAddress is required.")]
    public Uri BaseAddress { get; set; } = null!;

    [Required(ErrorMessage = "Timeout is required.")]
    public int TimeoutInSeconds { get; set; } = 30;

    [Required(ErrorMessage = "HubAddress is required.")]
    public string HubAddress { get; set; } = null!;
}
