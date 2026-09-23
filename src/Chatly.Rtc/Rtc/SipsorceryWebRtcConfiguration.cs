using Chatly.Rtc.Options;
using SIPSorcery.Net;

namespace Chatly.Rtc.Rtc;

internal static class SipsorceryWebRtcConfiguration
{
    internal static RTCConfiguration Create(RtcOption option) => new()
    {
        iceServers = option.IceServers
            .Select(static url => new RTCIceServer { urls = url })
            .ToList()
    };
}
