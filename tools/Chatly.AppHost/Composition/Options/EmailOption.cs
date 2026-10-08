using System.ComponentModel.DataAnnotations;

namespace Chatly.AppHost.Composition.Options;

internal sealed class EmailOption
{
    [Required(ErrorMessage = "Sender name is required.")]
    [StringLength(100, ErrorMessage = "Sender name must not exceed 100 characters.")]
    public string SenderName { get; init; } = null!;

    [Required(ErrorMessage = "Sender email is required.")]
    [EmailAddress(ErrorMessage = "Sender email must be a valid email address.")]
    public string SenderEmail { get; init; } = null!;

    public string? Username { get; init; }

    public string? Password { get; init; }

    public bool UseSsl { get; init; }
}