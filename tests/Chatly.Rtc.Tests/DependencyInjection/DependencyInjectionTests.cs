using Chatly.Rtc.Abstractions;
using Chatly.Rtc.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Chatly.Rtc.Tests.DependencyInjection;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddChatlyRtc_RegistersFactoryAsSingletonWithoutGlobalAudioTransport()
    {
        var services = new ServiceCollection();

        services.AddChatlyRtc();
        services.AddSingleton<IOptions<RtcOption>>(Microsoft.Extensions.Options.Options.Create(new RtcOption
        {
            IceServers = ["stun:one.example.test"]
        }));

        var descriptor = Assert.Single(services, static item => item.ServiceType == typeof(IWebRtcPeerFactory));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.DoesNotContain(services, static item => item.ServiceType == typeof(IRtcAudioTransport));

        using var provider = services.BuildServiceProvider();
        Assert.Same(
            provider.GetRequiredService<IWebRtcPeerFactory>(),
            provider.GetRequiredService<IWebRtcPeerFactory>());
    }
}
