using Chatly.Rtc.Models;

namespace Chatly.Rtc.Events;

public sealed class IceCandidateCreatedEventArgs(
    IceCandidate candidate) : EventArgs
{
    public IceCandidate Candidate { get; } = candidate;
}