using System.ComponentModel.DataAnnotations;

namespace Chatly.Shared.Options;

public abstract class Auth0OptionBase
{
    [Required(ErrorMessage = "Auth0 domain is required.")]
    public string Domain { get; init; } = null!;

    [Required(ErrorMessage = "Auth0 audience is required.")]
    [Url(ErrorMessage = "Audience must be a valid URL.")]
    public string Audience { get; init; } = null!;

    [Required(ErrorMessage = "Auth0 client id is required.")]
    public string ClientId { get; init; } = null!;
}