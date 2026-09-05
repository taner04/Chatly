using Chatly.Desktop.Abstraction.Settings;

namespace Chatly.Desktop.Services.Settings.DirectoryProviders;

internal sealed class WindowsSettingsDirectoryProvider : ISettingsDirectoryProvider
{
    public string GetRootDirectory()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }
}