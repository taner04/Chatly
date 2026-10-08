using Chatly.Desktop.Abstraction.Settings;
using Chatly.Desktop.Models.Settings.Theme;

namespace Chatly.Desktop.Models.Settings;

[SingletonService]
public sealed class AppSettings(ISettingsStore settingsStore)
{
    public NotificationSettings NotificationSettings { get; } = settingsStore.LoadSettings<NotificationSettings>();
    public ThemeSettings ThemeSettings { get; } = settingsStore.LoadSettings<ThemeSettings>();
    public CallSettings CallSettings { get; } = settingsStore.LoadSettings<CallSettings>();
    public DeviceSettings DeviceSettings { get; } = settingsStore.LoadSettings<DeviceSettings>();

    public string AppVersion { get; } =
        typeof(App).Assembly.GetName().Version?.ToString(3) ?? "Unknown";

    public string Platform { get; } = OperatingSystem.IsMacOS()
        ? "macOS"
        : OperatingSystem.IsWindows()
            ? "Windows"
            : Environment.OSVersion.Platform.ToString();

    public string DeviceName { get; } = Environment.MachineName;

    internal void Save()
    {
        settingsStore.SaveSettings(NotificationSettings);
        settingsStore.SaveSettings(ThemeSettings);
        settingsStore.SaveSettings(CallSettings);
        settingsStore.SaveSettings(DeviceSettings);
    }
}