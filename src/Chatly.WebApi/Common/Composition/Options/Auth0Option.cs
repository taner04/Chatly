using System.ComponentModel.DataAnnotations;

namespace Chatly.WebApi.Common.Composition.Options;

public sealed class Auth0Option
{
    [Required(ErrorMessage = "Auth0 domain is required.")]
    public string Domain { get; init; } = null!;

    [Required(ErrorMessage = "Auth0 audience is required.")]
    [Url(ErrorMessage = "Audience must be a valid URL.")]
    public string Audience { get; init; } = null!;

    [Required(ErrorMessage = "Auth0 client id is required.")]
    public string ClientId { get; init; } = null!;

    [Required(ErrorMessage = "Auth0 client secret is required.")]
    public string ClientSecret { get; init; } = null!;

    public bool UsePersistentStorage { get; init; }
}