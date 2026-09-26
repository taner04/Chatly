using System.Runtime.Versioning;
using Chatly.Desktop.Abstraction.Settings;

namespace Chatly.Desktop.Services.Settings.DirectoryProviders;

[SupportedOSPlatform("windows")]
internal sealed class WindowsSettingsDirectoryProvider : ISettingsDirectoryProvider
{
    public string GetRootDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
}