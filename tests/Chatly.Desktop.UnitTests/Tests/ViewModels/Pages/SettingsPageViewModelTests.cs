using Chatly.Desktop.Abstraction.Calls;
using Chatly.Desktop.Models.Calls;
using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.UnitTests.Tests.Services.Calls.TestDoubles;
using Chatly.Desktop.ViewModels.Pages;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chatly.Desktop.UnitTests.Tests.ViewModels.Pages;

public sealed class SettingsPageViewModelTests
{
    private static readonly CallAudioDevice[] Devices =
    [
        new("default", "Default - MacBook Air Microphone", CallAudioDeviceKind.Input),
        new("mic-1", "MacBook Air Microphone", CallAudioDeviceKind.Input),
        new("mic-2", "", CallAudioDeviceKind.Input),
        new("speaker-1", "Speakers", CallAudioDeviceKind.Output)
    ];

    [Fact]
    public async Task RefreshDevices_Should_ListSystemDefaultAndRealDevices_When_DevicesAreAvailable()
    {
        var viewModel = CreateViewModel(out _);

        await viewModel.RefreshDevicesCommand.ExecuteAsync(null);

        viewModel.InputDevices.Select(device => device.Id).Should().Equal(null, "mic-1", "mic-2");
        viewModel.InputDevices[2].Label.Should().Be("Unknown device");
        viewModel.OutputDevices.Select(device => device.Id).Should().Equal(null, "speaker-1");
        viewModel.SelectedInputDevice!.Id.Should().BeNull();
    }

    [Fact]
    public async Task SelectedInputDevice_Should_BeSaved_When_UserPicksDevice()
    {
        var viewModel = CreateViewModel(out var callSettings);
        await viewModel.RefreshDevicesCommand.ExecuteAsync(null);

        viewModel.SelectedInputDevice = viewModel.InputDevices.Single(device => device.Id == "mic-1");

        callSettings.InputDeviceId.Should().Be("mic-1");
    }

    [Fact]
    public async Task RefreshDevices_Should_KeepSavedDeviceAndSelectIt_When_DeviceIsPresent()
    {
        var viewModel = CreateViewModel(out var callSettings);
        callSettings.InputDeviceId = "mic-2";

        await viewModel.RefreshDevicesCommand.ExecuteAsync(null);

        viewModel.SelectedInputDevice!.Id.Should().Be("mic-2");
        callSettings.InputDeviceId.Should().Be("mic-2");
    }

    [Fact]
    public async Task RefreshDevices_Should_ShowSystemDefaultWithoutForgettingSavedDevice_When_DeviceIsUnplugged()
    {
        var viewModel = CreateViewModel(out var callSettings);
        callSettings.InputDeviceId = "usb-headset";

        await viewModel.RefreshDevicesCommand.ExecuteAsync(null);

        viewModel.SelectedInputDevice!.Id.Should().BeNull();
        callSettings.InputDeviceId.Should().Be("usb-headset");
    }

    private static SettingsPageViewModel CreateViewModel(out CallSettings callSettings)
    {
        var appSettings = CallSessionFactory.CreateAppSettings();
        callSettings = appSettings.CallSettings;
        var mediaHost = Substitute.For<ICallMediaHost>();
        mediaHost.GetAudioDevicesAsync(Arg.Any<CancellationToken>()).Returns(Devices);
        return new SettingsPageViewModel(appSettings, null!, mediaHost, NullLogger<SettingsPageViewModel>.Instance);
    }
}