using System.ComponentModel.DataAnnotations;
using Chatly.Shared.Attributes;

namespace Chatly.WebApi.Common.Composition.Options;

[Option]
internal sealed class IdentityAdminOption
{
    [Required(ErrorMessage = "Identity admin client id is required.")]
    public string ClientId { get; init; } = null!;

    [Required(ErrorMessage = "Identity admin client secret is required.")]
    public string ClientSecret { get; init; } = null!;
}