using Chatly.AppHost.Composition.Extensions.DistributedApplicationBuilder.Modules;

namespace Chatly.AppHost.Composition.Extensions.DistributedApplicationBuilder;

internal static class DistributedApplicationBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        internal IDistributedApplicationBuilder AddChatlyResources()
        {
            var keycloakApiClientSecret = builder.AddParameter("keycloak-api-client-secret", true);
            var liveKitApiSecret = builder.AddParameter("livekit-api-secret", true);

            var database = builder.AddChatlyDatabase();
            var migrations = builder.AddChatlyMigrations(database);
            var blobs = builder.AddChatlyBlobStorage();
            var papercut = builder.AddChatlyPapercut();
            var api = builder.AddChatlyWebApi();
            var keycloak = builder.AddChatlyKeycloak(papercut, api, keycloakApiClientSecret);
            var liveKit = builder.AddChatlyLiveKit(api, liveKitApiSecret);

            api
                .WithReference(database)
                .WaitFor(database)
                .WithReference(blobs)
                .WaitFor(blobs)
                .WaitForCompletion(migrations)
                .WithChatlyEmail(papercut)
                .WithChatlyIdentity(keycloak, keycloakApiClientSecret)
                .WithChatlyLiveKit(liveKit, liveKitApiSecret);

            return builder;
        }
    }
}
