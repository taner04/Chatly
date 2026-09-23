using Chatly.Rtc.Events;
using Chatly.Rtc.Models;

namespace Chatly.Rtc.Abstractions;

public interface IWebRtcPeer : IAsyncDisposable
{
    IRtcAudioTransport Audio { get; }

    event EventHandler<IceCandidateCreatedEventArgs>? IceCandidateCreated;

    event EventHandler<PeerConnectionStateChangedEventArgs>? StateChanged;

    Task<string> CreateOfferAsync();

    Task SetRemoteOfferAsync(string sdp);

    Task<string> CreateAnswerAsync();

    Task SetRemoteAnswerAsync(string sdp);

    Task AddIceCandidateAsync(IceCandidate candidate);

    Task CloseAsync();
}
