namespace Chatly.Audio.Models;

public sealed record AudioDeviceInfo(
    nint Id,
    string Name,
    bool IsDefault);