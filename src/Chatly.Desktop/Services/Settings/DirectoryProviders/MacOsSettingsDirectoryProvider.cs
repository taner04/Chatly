using System.IO;
using System.Runtime.Versioning;
using Chatly.Desktop.Abstraction.Settings;

namespace Chatly.Desktop.Services.Settings.DirectoryProviders;

[SupportedOSPlatform("macos")]
internal sealed class MacOsSettingsDirectoryProvider : ISettingsDirectoryProvider
{
    public string GetRootDirectory() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Library",
            "Application Support");
}