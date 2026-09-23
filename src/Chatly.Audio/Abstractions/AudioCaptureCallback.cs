using Chatly.Audio.Models;

namespace Chatly.Audio.Abstractions;

// The samples are borrowed from the backend and are valid only for the duration of the callback.
public delegate void AudioCaptureCallback(
    ReadOnlySpan<float> interleavedSamples,
    AudioFrameInfo frameInfo);