using Chatly.Desktop.Abstraction.Settings;

namespace Chatly.Desktop.Models.Settings;

public sealed partial class CallSettings : ObservableObject, ISettingsGroup
{
    [ObservableProperty] public partial string? InputDeviceId { get; set; }

    [ObservableProperty] public partial string? OutputDeviceId { get; set; }

    [ObservableProperty] public partial bool EchoCancellation { get; set; } = true;

    [ObservableProperty] public partial bool NoiseSuppression { get; set; } = true;

    [ObservableProperty] public partial bool AutoGainControl { get; set; } = true;

    public static string GroupName => "Call";
}