using System;
using System.ComponentModel.DataAnnotations;

namespace Chatly.Desktop.Options;

public sealed class WebApiClientOption
{
    [Required(ErrorMessage = "BaseAddress is required.")]
    public Uri BaseAddress { get; set; } = null!;

    [Required(ErrorMessage = "Timeout is required.")]
    public int TimeoutInSeconds { get; set; } = 30;

    [Required(ErrorMessage = "BaseAddress is required.")]
    public Uri HubAddress { get; set; } = null!;
}
