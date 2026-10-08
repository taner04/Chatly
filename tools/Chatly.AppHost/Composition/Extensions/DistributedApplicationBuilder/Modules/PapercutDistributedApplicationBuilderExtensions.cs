using Chatly.AppHost.Composition.Options;
using Chatly.ServiceDefaults;
using Chatly.Shared.Extensions;
using CommunityToolkit.Aspire.Hosting.PapercutSmtp;

namespace Chatly.AppHost.Composition.Extensions.DistributedApplicationBuilder.Modules;

internal static class PapercutDistributedApplicationBuilderExtensions
{
    private const string SmtpEndpointName = "smtp";

    extension(IDistributedApplicationBuilder builder)
    {
        internal IResourceBuilder<PapercutSmtpContainerResource> AddChatlyPapercut()
        {
            var option = builder.Configuration.GetOption<PapercutOption>();

            return builder.AddPapercutSmtp(AppHostConstants.Papercut, option.HttpPort, option.SmtpPort)
                .WithImage(option.Image, option.Tag);
        }
    }

    extension(IResourceBuilder<PapercutSmtpContainerResource> papercut)
    {
        internal EndpointReferenceExpression SmtpHost =>
            papercut.GetEndpoint(SmtpEndpointName).Property(EndpointProperty.Host);

        internal EndpointReferenceExpression SmtpPort =>
            papercut.GetEndpoint(SmtpEndpointName).Property(EndpointProperty.Port);
    }
}
