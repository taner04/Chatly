using Chatly.Audio.Abstractions;
using Chatly.Audio.Services.SoundFlow;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Audio.Tests.DependencyInjection;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddChatlyAudioRegistersExpectedSingletonMappings()
    {
        ServiceCollection services = [];

        services.AddChatlyAudio();

        AssertSingleton<IAudioAssetPlayer, SoundFlowAudioAssetPlayer>(services);
        AssertSingleton<IAudioDeviceManager, SoundFlowAudioDeviceManager>(services);
        AssertSingleton<IAudioHost, SoundFlowAudioHost>(services);
        AssertSingleton<IRealtimeAudioService, SoundFlowRealtimeAudioService>(services);
        AssertSingleton<SoundFlowRuntime, SoundFlowRuntime>(services);
        Assert.DoesNotContain(
            services,
            descriptor => descriptor.ServiceType == typeof(IRealtimeAudioSession));
    }

    private static void AssertSingleton<TService, TImplementation>(
        IServiceCollection services)
    {
        var descriptor = Assert.Single(
            services,
            candidate => candidate.ServiceType == typeof(TService));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Equal(typeof(TImplementation), descriptor.ImplementationType);
    }
}
