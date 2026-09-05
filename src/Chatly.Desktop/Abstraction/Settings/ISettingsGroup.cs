namespace Chatly.Desktop.Abstraction.Settings;

public interface ISettingsGroup
{
    static abstract string GroupName { get; }
}