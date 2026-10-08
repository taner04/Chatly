using System.ComponentModel.DataAnnotations;

namespace Chatly.AppHost.Composition.Options;

internal sealed class LiveKitOption
{
    [Required(ErrorMessage = "LiveKit image is required.")]
    public string Image { get; init; } = null!;

    [Required(ErrorMessage = "LiveKit image tag is required.")]
    public string Tag { get; init; } = null!;

    [Required(ErrorMessage = "LiveKit bind address is required.")]
    public string BindAddress { get; init; } = null!;

    [Required(ErrorMessage = "LiveKit node IP is required.")]
    public string NodeIp { get; init; } = null!;

    [Range(1024, 65535, ErrorMessage = "LiveKit HTTP port must be between 1024 and 65535.")]
    public int HttpPort { get; init; }

    [Range(1024, 65535, ErrorMessage = "LiveKit RTC TCP port must be between 1024 and 65535.")]
    public int RtcTcpPort { get; init; }

    [Range(1024, 65535, ErrorMessage = "LiveKit RTC UDP port must be between 1024 and 65535.")]
    public int RtcUdpPort { get; init; }

    [Required(ErrorMessage = "LiveKit API key is required.")]
    public string ApiKey { get; init; } = null!;

    public string ServerUrl => $"ws://{NodeIp}:{HttpPort}";
}