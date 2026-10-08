using System.ComponentModel.DataAnnotations;
using Chatly.Shared.Attributes;
using Chatly.Shared.Options;

namespace Chatly.Desktop.Options;

[Option]
public sealed class OidcOption : OidcOptionBase
{
    [Required(ErrorMessage = "OIDC scope is required.")]
    public string Scope { get; init; } = null!;

    [Required(ErrorMessage = "OIDC redirect URI is required.")]
    public string RedirectUri { get; init; } = null!;

    public string? Prompt { get; init; }
}