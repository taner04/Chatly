using System.ComponentModel.DataAnnotations;

namespace Chatly.Desktop.Options;

public sealed class Auth0Option
{
    [Required(ErrorMessage = "Auth0 domain is required.")]
    public string Domain { get; init; } = null!;

    [Required(ErrorMessage = "Auth0 client id is required.")]
    public string ClientId { get; init; } = null!;

    [Required(ErrorMessage = "Auth0 audience is required.")]
    [Url(ErrorMessage = "Audience must be a valid URL.")]
    public string Audience { get; init; } = null!;

    [Required(ErrorMessage = "Auth0 connection name is required.")]
    public string ConnectionName { get; init; } = null!;

    [Required(ErrorMessage = "Auth0 scope is required.")]
    public string Scope { get; init; } = null!;

    [Required(ErrorMessage = "Auth0 redirect URI is required.")]
    public string RedirectUri { get; init; } = null!;
}
