using Chatly.WebApi.Common.Composition.Options;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace Chatly.WebApi.Common.Composition.Configs.OpenApi;

internal sealed class BearerDocumentTransformer(IOptions<Auth0Option> auth0Options) : IOpenApiDocumentTransformer
{
    private readonly Auth0Option _auth0Config = auth0Options.Value;

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
            Description = "Auth0 OAuth2 Login",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"https://{_auth0Config.Domain}/authorize"),
                    TokenUrl = new Uri($"https://{_auth0Config.Domain}/oauth/token"),
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