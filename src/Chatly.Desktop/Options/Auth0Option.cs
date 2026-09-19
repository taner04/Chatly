using System.ComponentModel.DataAnnotations;
using Chatly.Shared.Attributes;
using Chatly.Shared.Options;

namespace Chatly.Desktop.Options;

[Option]
public sealed class Auth0Option : Auth0OptionBase
{
    [Required(ErrorMessage = "Auth0 connection name is required.")]
    public string ConnectionName { get; init; } = null!;

    [Required(ErrorMessage = "Auth0 scope is required.")]
    public string Scope { get; init; } = null!;

    [Required(ErrorMessage = "Auth0 redirect URI is required.")]
    public string RedirectUri { get; init; } = null!;

    public string? Prompt { get; init; }
}