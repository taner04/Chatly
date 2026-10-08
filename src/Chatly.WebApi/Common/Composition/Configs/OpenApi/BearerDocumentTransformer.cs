using Chatly.WebApi.Common.Composition.Options;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace Chatly.WebApi.Common.Composition.Configs.OpenApi;

internal sealed class BearerDocumentTransformer(IOptions<OidcOption> oidcOptions) : IOpenApiDocumentTransformer
{
    private readonly OidcOption _oidcOption = oidcOptions.Value;

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["JWT"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Bearer Token"
        };

        document.Components.SecuritySchemes["OAuth"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "Keycloak OAuth2 Login",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(_oidcOption.AuthorizationEndpoint),
                    TokenUrl = new Uri(_oidcOption.TokenEndpoint),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "OpenID Connect scope" },
                        { "profile", "Profile information scope" },
                        { "email", "Email information scope" }
                    }
                }
            }
        };

        return Task.CompletedTask;
    }
}