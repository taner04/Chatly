using System.ComponentModel.DataAnnotations;
using Chatly.Shared.Attributes;
using Chatly.Shared.Options;

namespace Chatly.WebApi.Common.Composition.Options;

[Option]
internal sealed class OidcOption : OidcOptionBase
{
    [Required(ErrorMessage = "OIDC audience is required.")]
    public string Audience { get; init; } = null!;

    [Required(ErrorMessage = "OIDC back-channel logout audience is required.")]
    public string BackchannelLogoutAudience { get; init; } = null!;

    public bool UsePersistentStorage { get; init; }

    public string AuthorizationEndpoint => $"{Authority}/protocol/openid-connect/auth";

    public string TokenEndpoint => $"{Authority}/protocol/openid-connect/token";

    public string SessionsAdminEndpoint => $"{Authority.Replace("/realms/", "/admin/realms/")}/sessions";
}