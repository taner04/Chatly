using Chatly.Contracts.Features.DeviceSessions.Models;
using Chatly.Contracts.Features.DeviceSessions.Notifications;
using Chatly.Desktop.Abstraction.Calls;
using Chatly.Desktop.Abstraction.Popups;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Models.Calls;
using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.Services.Api.Clients;
using Chatly.Desktop.Services.Api.Hubs.NotificationHub.NotificationHandlers;
using Chatly.Desktop.Services.Api.Refit;
using Chatly.Desktop.UnitTests.Infrastructure;
using Chatly.Desktop.UnitTests.Tests.Services.Calls.TestDoubles;
using Chatly.Desktop.ViewModels.Pages;
using Chatly.Desktop.ViewModels.Popups;
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

    private readonly IChatlyApi _api = Substitute.For<IChatlyApi>();
    private readonly IPopupService _popupService = Substitute.For<IPopupService>();

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

    [Fact]
    public async Task RefreshDeviceSessions_Should_ListCurrentDeviceFirst_When_SessionsAreLoaded()
    {
        var current = Session("MacBook", true, DateTimeOffset.UtcNow.AddHours(-2));
        var other = Session("Desktop-PC", false, DateTimeOffset.UtcNow);
        _api.GetDeviceSessionsAsync(Arg.Any<CancellationToken>())
            .Returns(ApiResponses.Ok<IReadOnlyList<DeviceSessionContract>>([other, current]));
        var viewModel = CreateViewModel(out _);

        await viewModel.RefreshDeviceSessionsCommand.ExecuteAsync(null);

        viewModel.DeviceSessions.Select(session => session.SessionId)
            .Should().Equal(current.SessionId, other.SessionId);
        viewModel.HasOtherDeviceSessions.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeDeviceSession_Should_RemoveSession_When_RequestSucceeds()
    {
        var current = Session("MacBook", true, DateTimeOffset.UtcNow);
        var other = Session("Desktop-PC", false, DateTimeOffset.UtcNow);
        _api.GetDeviceSessionsAsync(Arg.Any<CancellationToken>())
            .Returns(ApiResponses.Ok<IReadOnlyList<DeviceSessionContract>>([current, other]));
        _api.RevokeDeviceSessionAsync(other.SessionId, Arg.Any<CancellationToken>())
            .Returns(ApiResponses.Ok<object>(null));
        AnswerConfirmation(true);
        var viewModel = CreateViewModel(out _);
        await viewModel.RefreshDeviceSessionsCommand.ExecuteAsync(null);

        await viewModel.RevokeDeviceSessionCommand.ExecuteAsync(viewModel.DeviceSessions[1]);

        viewModel.DeviceSessions.Select(session => session.SessionId).Should().Equal(current.SessionId);
        viewModel.HasOtherDeviceSessions.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeDeviceSession_Should_KeepSession_When_UserCancelsConfirmation()
    {
        var current = Session("MacBook", true, DateTimeOffset.UtcNow);
        var other = Session("Desktop-PC", false, DateTimeOffset.UtcNow);
        _api.GetDeviceSessionsAsync(Arg.Any<CancellationToken>())
            .Returns(ApiResponses.Ok<IReadOnlyList<DeviceSessionContract>>([current, other]));
        AnswerConfirmation(false);
        var viewModel = CreateViewModel(out _);
        await viewModel.RefreshDeviceSessionsCommand.ExecuteAsync(null);

        await viewModel.RevokeDeviceSessionCommand.ExecuteAsync(viewModel.DeviceSessions[1]);

        await _api.DidNotReceive().RevokeDeviceSessionAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        viewModel.DeviceSessions.Should().HaveCount(2);
    }

    [Fact]
    public Task DeviceSessionsChanged_Should_ReloadDeviceSessions_When_AnotherDeviceSignsIn() =>
        UiThread.RunAsync(async () =>
        {
            var current = Session("MacBook", true, DateTimeOffset.UtcNow);
            var newDevice = Session("Desktop-PC", false, DateTimeOffset.UtcNow);
            _api.GetDeviceSessionsAsync(Arg.Any<CancellationToken>())
                .Returns(ApiResponses.Ok<IReadOnlyList<DeviceSessionContract>>([current, newDevice]));
            var viewModel = CreateViewModel(out _);

            await new DeviceSessionsChangedNotificationHandler(viewModel)
                .HandleAsync(new DeviceSessionsChangedNotification());

            viewModel.DeviceSessions.Select(session => session.SessionId)
                .Should().Equal(current.SessionId, newDevice.SessionId);
        });

    private void AnswerConfirmation(bool confirmed) =>
        _popupService
            .When(service => service.ShowAsync(Arg.Any<IPopupViewModel>(), Arg.Any<CancellationToken>()))
            .Do(call =>
            {
                var popup = call.Arg<IPopupViewModel>().Should().BeOfType<ConfirmationPopupViewModel>().Subject;
                if (confirmed)
                {
                    popup.ConfirmCommand.Execute(null);
                }
                else
                {
                    popup.CancelCommand.Execute(null);
                }
            });

    private static DeviceSessionContract Session(string deviceName, bool isCurrent, DateTimeOffset lastSeenAt) =>
        new(Guid.NewGuid(), deviceName, "macOS", "1.0.0", lastSeenAt, lastSeenAt, isCurrent);

    private SettingsPageViewModel CreateViewModel(out CallSettings callSettings)
    {
        var appSettings = CallSessionFactory.CreateAppSettings();
        callSettings = appSettings.CallSettings;
        var mediaHost = Substitute.For<ICallMediaHost>();
        mediaHost.GetAudioDevicesAsync(Arg.Any<CancellationToken>()).Returns(Devices);
        return new SettingsPageViewModel(
            appSettings,
            null!,
            mediaHost,
            new DeviceSessionApiClient(_api),
            Substitute.For<IToastService>(),
            _popupService,
            NullLogger<SettingsPageViewModel>.Instance);
    }
}