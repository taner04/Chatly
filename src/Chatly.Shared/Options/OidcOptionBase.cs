using System.ComponentModel.DataAnnotations;

namespace Chatly.Shared.Options;

public abstract class OidcOptionBase
{
    [Required(ErrorMessage = "OIDC authority is required.")]
    [Url(ErrorMessage = "OIDC authority must be a valid URL.")]
    public string Authority { get; init; } = null!;

    [Required(ErrorMessage = "OIDC client id is required.")]
    public string ClientId { get; init; } = null!;
}