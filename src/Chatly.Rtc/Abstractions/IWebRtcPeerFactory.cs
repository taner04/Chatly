namespace Chatly.Rtc.Abstractions;

public interface IWebRtcPeerFactory
{
    Task<IWebRtcPeer> CreatePeer(CancellationToken cancellationToken = default);
}