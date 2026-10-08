using Chatly.Shared.Extensions;
using Chatly.WebApi.Common.Composition.Options;
using Chatly.WebApi.Common.Infrastructure.Blob;
using Hangfire;
using Hangfire.Dashboard;
using Scalar.AspNetCore;

namespace Chatly.WebApi.Common.Composition.Extensions;

internal static class WebApplicationExtensions
{
    private static readonly string[] ServerCallbackRoutes =
    [
        ApiRoutes.Identity.BackchannelLogout,
        ApiRoutes.LiveKit.Webhook
    ];

    extension(WebApplication app)
    {
        internal WebApplication UseHttpsRedirectionForClients()
        {
            app.UseWhen(
                context => !ServerCallbackRoutes.Any(route => context.Request.Path.StartsWithSegments(route)),
                branch => branch.UseHttpsRedirection());

            return app;
        }

        internal WebApplication MapScalar()
        {
            var oidc = app.Configuration.GetOption<OidcOption>();

            app.MapScalarApiReference(options =>
            {
                options.Layout = ScalarLayout.Classic;
                options.Theme = ScalarTheme.DeepSpace;
                options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.RestSharp);
                options.AddPreferredSecuritySchemes("OAuth")
                    .AddOAuth2Authentication("OAuth", scheme => scheme
                        .WithFlows(flows => flows
                            .WithAuthorizationCode(flow => flow
                                .WithAuthorizationUrl(oidc.AuthorizationEndpoint)
                                .WithTokenUrl(oidc.TokenEndpoint)
                                .WithClientId(oidc.ClientId)
                                .WithPkce(Pkce.Sha256)))
                        .WithDefaultScopes("openid", "profile", "email"));

                if (oidc.UsePersistentStorage)
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
                .Select(type => (IEndpoint)Activator.CreateInstance(type, true)!);

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


        internal WebApplication AddHangfireDashboard()
        {
            app.UseHangfireDashboard(options: new DashboardOptions
            {
                DarkModeEnabled = true,
                Authorization = [new LocalRequestsOnlyAuthorizationFilter()]
            });

            return app;
        }
    }
}