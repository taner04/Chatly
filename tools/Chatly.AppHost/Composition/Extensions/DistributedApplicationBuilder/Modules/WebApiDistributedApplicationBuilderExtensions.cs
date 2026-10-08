using Chatly.AppHost.Composition.Options;
using Chatly.ServiceDefaults;
using Chatly.Shared.Extensions;
using CommunityToolkit.Aspire.Hosting.PapercutSmtp;
using Projects;

namespace Chatly.AppHost.Composition.Extensions.DistributedApplicationBuilder.Modules;

internal static class WebApiDistributedApplicationBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        internal IResourceBuilder<ProjectResource> AddChatlyWebApi() =>
            builder.AddProject<Chatly_WebApi>(AppHostConstants.WebApiResourceName);
    }

    extension(IResourceBuilder<ProjectResource> api)
    {
        internal IResourceBuilder<ProjectResource> WithChatlyEmail(
            IResourceBuilder<PapercutSmtpContainerResource> papercut)
        {
            var option = api.ApplicationBuilder.Configuration.GetOption<EmailOption>();

            return api
                .WithEnvironment("EmailOption__Host", papercut.SmtpHost)
                .WithEnvironment("EmailOption__Port", papercut.SmtpPort)
                .WithEnvironment("EmailOption__SenderName", option.SenderName)
                .WithEnvironment("EmailOption__SenderEmail", option.SenderEmail)
                .WithEnvironment("EmailOption__Username", option.Username ?? string.Empty)
                .WithEnvironment("EmailOption__Password", option.Password ?? string.Empty)
                .WithEnvironment("EmailOption__UseSsl", option.UseSsl.ToString())
                .WaitFor(papercut);
        }

        internal IResourceBuilder<ProjectResource> WithChatlyIdentity(
            IResourceBuilder<KeycloakResource> keycloak,
            IResourceBuilder<ParameterResource> apiClientSecret) =>
            api
                .WithEnvironment("IdentityAdminOption__ClientSecret", apiClientSecret)
                .WaitFor(keycloak);

        internal IResourceBuilder<ProjectResource> WithChatlyLiveKit(
            IResourceBuilder<ContainerResource> liveKit,
            IResourceBuilder<ParameterResource> apiSecret)
        {
            var option = api.ApplicationBuilder.Configuration.GetOption<LiveKitOption>();

            return api
                .WithEnvironment("LiveKitOption__ServerUrl", option.ServerUrl)
                .WithEnvironment("LiveKitOption__ApiKey", option.ApiKey)
                .WithEnvironment("LiveKitOption__ApiSecret", apiSecret)
                .WaitFor(liveKit);
        }
    }
}
