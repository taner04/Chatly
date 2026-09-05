using System.IO;
using Chatly.Desktop.Abstraction.Settings;

namespace Chatly.Desktop.Services.Settings.DirectoryProviders;

internal sealed class MacOsSettingsDirectoryProvider : ISettingsDirectoryProvider
{
    public string GetRootDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Library",
            "Application Support");
    }
}