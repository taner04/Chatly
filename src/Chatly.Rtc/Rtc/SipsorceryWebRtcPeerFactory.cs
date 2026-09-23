using Chatly.DependencyInjection;
using Chatly.Rtc.Abstractions;
using Chatly.Rtc.Options;
using Microsoft.Extensions.Options;

namespace Chatly.Rtc.Rtc;

[SingletonService(typeof(IWebRtcPeerFactory))]
internal sealed class SipsorceryWebRtcPeerFactory(IOptions<RtcOption> options) : IWebRtcPeerFactory
{
    public Task<IWebRtcPeer> CreatePeer(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IWebRtcPeer>(new SipsorceryWebRtcPeer(options.Value));
    }
}
