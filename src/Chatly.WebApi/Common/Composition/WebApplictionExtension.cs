using Chatly.Contracts.Extensions;
using Chatly.WebApi.Common.Abstractions;
using Chatly.WebApi.Common.Composition.Options;
using Scalar.AspNetCore;

namespace Chatly.WebApi.Common.Composition;

internal static class WebApplictionExtension
{
    extension(WebApplication app)
    {
          internal WebApplication MapScalar()
        {
            var auth0Config = app.Configuration.GetOption<Auth0Option>();

            app.MapScalarApiReference(opt =>
            {
                opt.Layout = ScalarLayout.Classic;
                opt.Theme = ScalarTheme.DeepSpace;
                opt.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.RestSharp);
                opt.AddPreferredSecuritySchemes("OAuth2")
                    .AddOAuth2Authentication("OAuth2", scheme => scheme
                        .WithFlows(flows => flows
                            .WithAuthorizationCode(flow => flow
                                .WithAuthorizationUrl($"https://{auth0Config.Domain}/authorize")
                                .WithTokenUrl($"https://{auth0Config.Domain}/oauth/token")
                                .WithClientId(auth0Config.ClientId)
                                .WithClientSecret(auth0Config.ClientSecret)
                                .WithPkce(Pkce.Sha256)
                                .AddQueryParameter("audience", auth0Config.Audience)
                            ))
                        .WithDefaultScopes("openid", "profile", "email", "email_verified")
                    );

                if (auth0Config.UsePersistentStorage)
                {
                    opt.EnablePersistentAuthentication();
                }
            });

            return app;
        }

        public WebApplication MapEndpoints()
        {
            var endpoints = typeof(Program).Assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IEndpoint).IsAssignableFrom(t))
                .Select(t => (IEndpoint)Activator.CreateInstance(t)!)
                .ToList();

            foreach (var endpoint in endpoints)
            {
                endpoint.MapEndpoint(app);
            }

            return app;
        }
        
    }
}