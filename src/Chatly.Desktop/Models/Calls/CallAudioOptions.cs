namespace Chatly.Desktop.Models.Calls;

public sealed record CallAudioOptions(
    string? InputDeviceId,
    string? OutputDeviceId,
    bool EchoCancellation,
    bool NoiseSuppression,
    bool AutoGainControl);