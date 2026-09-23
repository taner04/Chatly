namespace Chatly.Rtc.Models;

public sealed record IceCandidate(
    string Candidate,
    string? SdpMid,
    ushort? SdpMLineIndex);