using System.Net.Sockets;
using Chatly.AppHost.Composition.Options;
using Chatly.Contracts.Common;
using Chatly.ServiceDefaults;
using Chatly.Shared.Extensions;

namespace Chatly.AppHost.Composition.Extensions.DistributedApplicationBuilder.Modules;

internal static class LiveKitDistributedApplicationBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        internal IResourceBuilder<ContainerResource> AddChatlyLiveKit(
            IResourceBuilder<ProjectResource> api,
            IResourceBuilder<ParameterResource> apiSecret)
        {
            var option = builder.Configuration.GetOption<LiveKitOption>();

            return builder.AddContainer(AppHostConstants.LiveKitResourceName, option.Image, option.Tag)
                .WithArgs("--dev", "--bind", option.BindAddress, "--node-ip", option.NodeIp)
                .WithEnvironment("LIVEKIT_KEYS", ReferenceExpression.Create($"{option.ApiKey}: {apiSecret.Resource}"))
                .WithEnvironment(
                    "LIVEKIT_CONFIG",
                    ReferenceExpression.Create(
                        $"webhook:\n  api_key: {option.ApiKey}\n  urls:\n    - {api.GetEndpoint("http")}{ApiRoutes.LiveKit.Webhook}\n"))
                .WithHttpEndpoint(option.HttpPort, option.HttpPort, "http", isProxied: false)
                .WithEndpoint(option.RtcTcpPort, option.RtcTcpPort, "tcp", "rtc-tcp", isProxied: false)
                .WithEndpoint(
                    option.RtcUdpPort,
                    option.RtcUdpPort,
                    "udp",
                    "rtc-udp",
                    isProxied: false,
                    protocol: ProtocolType.Udp);
        }
    }
}
