using Chatly.Desktop.Abstraction.Settings;
using Chatly.Desktop.Models.Settings.Theme;

namespace Chatly.Desktop.Models.Settings;

[SingletonService]
public sealed class AppSettings(ISettingsStore settingsStore)
{
    public NotificationSettings NotificationSettings { get; } = settingsStore.LoadSettings<NotificationSettings>();
    public ThemeSettings ThemeSettings { get; } = settingsStore.LoadSettings<ThemeSettings>();

    internal void Save()
    {
        settingsStore.SaveSettings(NotificationSettings);
        settingsStore.SaveSettings(ThemeSettings);
    }
}
