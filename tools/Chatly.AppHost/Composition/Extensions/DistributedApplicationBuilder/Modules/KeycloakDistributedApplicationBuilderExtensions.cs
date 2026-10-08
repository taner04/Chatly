using Chatly.AppHost.Composition.Options;
using Chatly.Contracts.Common;
using Chatly.ServiceDefaults;
using Chatly.Shared.Extensions;
using CommunityToolkit.Aspire.Hosting.PapercutSmtp;

namespace Chatly.AppHost.Composition.Extensions.DistributedApplicationBuilder.Modules;

internal static class KeycloakDistributedApplicationBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        internal IResourceBuilder<KeycloakResource> AddChatlyKeycloak(
            IResourceBuilder<PapercutSmtpContainerResource> papercut,
            IResourceBuilder<ProjectResource> api,
            IResourceBuilder<ParameterResource> apiClientSecret)
        {
            var option = builder.Configuration.GetOption<KeycloakOption>();
            var emailOption = builder.Configuration.GetOption<EmailOption>();

            var keycloak = builder.AddKeycloak(
                    AppHostConstants.KeycloakResourceName,
                    option.Port,
                    builder.AddParameter("keycloak-admin-username"),
                    builder.AddParameter("keycloak-admin-password", true))
                .WithImageRegistry(option.Registry)
                .WithImage(option.Image, option.Tag)
                .WithRealmImport(option.RealmImportPath)
                .WithBindMount(option.ThemesPath, "/opt/keycloak/themes", true)
                .WithEnvironment("CHATLY_API_CLIENT_SECRET", apiClientSecret)
                .WithEnvironment("CHATLY_SEED_USER_PASSWORD", builder.AddParameter("keycloak-seed-user-password", true))
                .WithEnvironment("CHATLY_SMTP_HOST", papercut.SmtpHost)
                .WithEnvironment("CHATLY_SMTP_PORT", papercut.SmtpPort)
                .WithEnvironment("CHATLY_SMTP_FROM", emailOption.SenderEmail)
                .WithEnvironment("CHATLY_SMTP_FROM_NAME", emailOption.SenderName)
                .WithEnvironment(
                    "CHATLY_BACKCHANNEL_LOGOUT_URL",
                    ReferenceExpression.Create($"{api.GetEndpoint("http")}{ApiRoutes.Identity.BackchannelLogout}"))
                .WaitFor(papercut);

            return option.PersistData ? keycloak.WithDataVolume() : keycloak;
        }
    }
}
