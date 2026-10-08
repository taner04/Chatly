using Chatly.Desktop.Abstraction.Settings;
using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.Options;
using Chatly.Desktop.Services.Settings;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chatly.Desktop.UnitTests.Tests.Services.Settings;

public sealed class JsonSettingsStoreTests : IDisposable
{
    private readonly string _root =
        Path.Combine(Path.GetTempPath(), "chatly-settings-tests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }

    [Fact]
    public void LoadSettings_Should_ReturnDefaults_When_NoFileExists()
    {
        var settings = CreateStore().LoadSettings<CallSettings>();

        settings.InputDeviceId.Should().BeNull();
        settings.EchoCancellation.Should().BeTrue();
        settings.NoiseSuppression.Should().BeTrue();
        settings.AutoGainControl.Should().BeTrue();
    }

    [Fact]
    public void SaveSettings_Should_RoundTripValues_When_LoadedAgain()
    {
        var store = CreateStore();
        store.SaveSettings(new CallSettings { InputDeviceId = "mic-2", NoiseSuppression = false });

        var loaded = CreateStore().LoadSettings<CallSettings>();

        loaded.InputDeviceId.Should().Be("mic-2");
        loaded.NoiseSuppression.Should().BeFalse();
        loaded.EchoCancellation.Should().BeTrue();
    }

    [Fact]
    public void LoadSettings_Should_ReturnDefaults_When_FileIsCorrupted()
    {
        var directory = Path.Combine(_root, "Chatly", "Settings");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, $"{CallSettings.GroupName}.json"), "{ not json");

        var settings = CreateStore().LoadSettings<CallSettings>();

        settings.EchoCancellation.Should().BeTrue();
    }

    [Fact]
    public void SaveSettings_Should_KeepProfilesSeparate_When_ProfilesDiffer()
    {
        CreateStore("primary").SaveSettings(new DeviceSettings { DeviceId = Guid.NewGuid() });

        var secondary = CreateStore("secondary").LoadSettings<DeviceSettings>();

        secondary.DeviceId.Should().BeNull();
    }

    private JsonSettingsStore CreateStore(string profileName = DesktopProfileOption.DefaultName) =>
        new(
            new FixedDirectoryProvider(_root),
            Microsoft.Extensions.Options.Options.Create(new DesktopProfileOption { Name = profileName }),
            NullLogger<JsonSettingsStore>.Instance);

    private sealed class FixedDirectoryProvider(string root) : ISettingsDirectoryProvider
    {
        public string GetRootDirectory() => root;
    }
}