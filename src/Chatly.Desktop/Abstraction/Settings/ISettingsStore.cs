namespace Chatly.Desktop.Abstraction.Settings;

public interface ISettingsStore
{
    void SaveSettings<T>(T settings) where T : class, ISettingsGroup;
    T LoadSettings<T>() where T : class, ISettingsGroup, new();
}