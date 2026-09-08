using Chatly.Shared.Extensions;
using Chatly.WebApi.Common.Composition.Options;
using Scalar.AspNetCore;

namespace Chatly.WebApi.Common.Composition.Extensions;

internal static class WebApplicationExtensions
{
    extension(WebApplication app)
    {
        internal WebApplication MapScalar()
        {
            var auth0 = app.Configuration.GetOption<Auth0Option>();

            app.MapScalarApiReference(options =>
            {
                options.Layout = ScalarLayout.Classic;
                options.Theme = ScalarTheme.DeepSpace;
                options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.RestSharp);
                options.AddPreferredSecuritySchemes("OAuth")
                    .AddOAuth2Authentication("OAuth", scheme => scheme
                        .WithFlows(flows => flows
                            .WithAuthorizationCode(flow => flow
                                .WithAuthorizationUrl($"https://{auth0.Domain}/authorize")
                                .WithTokenUrl($"https://{auth0.Domain}/oauth/token")
                                .WithClientId(auth0.ClientId)
                                .WithPkce(Pkce.Sha256)
                                .AddQueryParameter("audience", auth0.Audience)))
                        .WithDefaultScopes("openid", "profile", "email"));

                if (auth0.UsePersistentStorage)
                {
                    options.EnablePersistentAuthentication();
                }
            });

            return app;
        }

        internal WebApplication MapEndpoints()
        {
            var endpoints = typeof(Program).Assembly.GetTypes()
                .Where(type =>
                    type is { IsClass: true, IsAbstract: false } &&
                    typeof(IEndpoint).IsAssignableFrom(type))
                .Select(type => (IEndpoint)Activator.CreateInstance(type, nonPublic: true)!);

            foreach (var endpoint in endpoints)
            {
                endpoint.MapEndpoint(app);
            }

            return app;
        }

        internal async Task<WebApplication> InitializeBlobStorage()
        {
            using var scope = app.Services.CreateScope();
            var attachmentService = scope.ServiceProvider.GetRequiredService<AzureBlobService>();

            await attachmentService.InitializeAsync();

            return app;
        }
    }
}
