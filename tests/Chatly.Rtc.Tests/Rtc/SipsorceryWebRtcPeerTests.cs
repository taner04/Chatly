using Chatly.Rtc.Abstractions;
using Chatly.Rtc.Options;
using Chatly.Rtc.Rtc;
using Microsoft.Extensions.Options;

namespace Chatly.Rtc.Tests.Rtc;

public sealed class SipsorceryWebRtcPeerTests
{
    private static readonly RtcOption OfflineOptions = new() { IceServers = [] };

    [Fact]
    public async Task Factory_RejectsCancellationBeforeCreatingPeer()
    {
        var factory = CreateFactory();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => factory.CreatePeer(cancellation.Token));
    }

    [Fact]
    public async Task Factory_CreatesIndependentPeers()
    {
        var factory = CreateFactory();
        await using var first = await factory.CreatePeer();
        await using var second = await factory.CreatePeer();

        Assert.NotSame(first, second);
        Assert.NotSame(first.Audio, second.Audio);
    }

    [Fact]
    public async Task CloseAndDispose_AreIdempotent()
    {
        var peer = await CreateFactory().CreatePeer();

        var firstClose = peer.CloseAsync();
        var secondClose = peer.CloseAsync();

        Assert.Same(firstClose, secondClose);
        await firstClose;
        await peer.DisposeAsync();
        await peer.DisposeAsync();
    }

    [Fact]
    public async Task CreateOffer_AdvertisesOpusAndG711Audio()
    {
        await using var peer = await CreateFactory().CreatePeer();

        var sdp = await peer.CreateOfferAsync();

        Assert.Contains("m=audio", sdp, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("opus/48000", sdp, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("PCMU/8000", sdp, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("PCMA/8000", sdp, StringComparison.OrdinalIgnoreCase);

        var lines = sdp.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var audioLine = Assert.Single(lines, static line => line.StartsWith("m=audio ", StringComparison.OrdinalIgnoreCase));
        var opusLine = Assert.Single(lines, static line => line.Contains("opus/48000", StringComparison.OrdinalIgnoreCase));
        var preferredPayload = audioLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)[3];
        var opusPayload = opusLine["a=rtpmap:".Length..].Split(' ', 2)[0];
        Assert.Equal(opusPayload, preferredPayload);
    }

    [Fact]
    public async Task ClosedPeer_DoesNotPoisonFutureFactoryCreations()
    {
        var factory = CreateFactory();
        var closedPeer = await factory.CreatePeer();
        await closedPeer.CloseAsync();
        await closedPeer.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => closedPeer.CreateOfferAsync());

        await using var nextPeer = await factory.CreatePeer();
        var sdp = await nextPeer.CreateOfferAsync();
        Assert.Contains("m=audio", sdp, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Configuration_PreservesMultipleIceUrlsAsSeparateServers()
    {
        string[] urls = ["stun:one.example.test", "turn:two.example.test?transport=tcp"];

        var configuration = SipsorceryWebRtcConfiguration.Create(new RtcOption { IceServers = urls });

        Assert.Equal(urls, configuration.iceServers.Select(static server => server.urls));
        Assert.DoesNotContain(configuration.iceServers, static server => server.urls.Contains(';'));
    }

    private static IWebRtcPeerFactory CreateFactory() =>
        new SipsorceryWebRtcPeerFactory(Microsoft.Extensions.Options.Options.Create(OfflineOptions));
}
