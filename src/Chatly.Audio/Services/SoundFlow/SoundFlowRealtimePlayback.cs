using SoundFlow.Components;

namespace Chatly.Audio.Services.SoundFlow;

internal sealed record SoundFlowRealtimePlayback(
    SoundPlayer Player,
    BufferedQueueDataProvider Provider,
    int Capacity);
