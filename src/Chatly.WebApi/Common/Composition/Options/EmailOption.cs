using System.ComponentModel.DataAnnotations;
using Chatly.Shared.Attributes;

namespace Chatly.WebApi.Common.Composition.Options;

[Option]
public sealed class EmailOption
{
    [Required(ErrorMessage = "SMTP server is required.")]
    [RegularExpression(
        @"^[a-zA-Z0-9.-]+$",
        ErrorMessage = "SMTP server must be a valid hostname without protocol or path.")]
    public string Host { get; set; } = null!;

    [Required(ErrorMessage = "Port is required.")]
    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535.")]
    public int Port { get; set; }

    [Required(ErrorMessage = "Sender name is required.")]
    [StringLength(100, ErrorMessage = "Sender name must not exceed 100 characters.")]
    public string SenderName { get; set; } = null!;

    [Required(ErrorMessage = "Sender email is required.")]
    [EmailAddress(ErrorMessage = "Sender email must be a valid email address.")]
    public string SenderEmail { get; set; } = null!;

    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool UseSsl { get; set; }
}