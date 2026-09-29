using System.ComponentModel.DataAnnotations;

namespace Chatly.AppHost.Options;

internal sealed class PapercutOption
{
    [Required(ErrorMessage = "Papercut image is required.")]
    public string Image { get; init; } = null!;

    [Required(ErrorMessage = "Papercut image tag is required.")]
    public string Tag { get; init; } = null!;

    [Range(1024, 65535, ErrorMessage = "Papercut HTTP port must be between 1024 and 65535.")]
    public int HttpPort { get; init; }

    [Range(1024, 65535, ErrorMessage = "Papercut SMTP port must be between 1024 and 65535.")]
    public int SmtpPort { get; init; }
}