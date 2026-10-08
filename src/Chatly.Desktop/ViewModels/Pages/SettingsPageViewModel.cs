using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Chatly.Contracts.Features.DeviceSessions.Models;
using Chatly.Desktop.Abstraction.Calls;
using Chatly.Desktop.Models.Calls;
using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.Models.Settings.Theme;
using Chatly.Desktop.Services.Api.Clients;
using Chatly.Desktop.Services.Authentication;
using AccentColor = Chatly.Desktop.Models.Settings.Theme.AccentColor;
using ThemeSettings = Chatly.Desktop.Models.Settings.Theme.ThemeSettings;

namespace Chatly.Desktop.ViewModels.Pages;

[SingletonService]
public sealed partial class SettingsPageViewModel(
    AppSettings appSettings,
    SessionService sessionService,
    ICallMediaHost callMediaHost,
    DeviceSessionApiClient deviceSessionApiClient,
    IToastService toastService,
    IPopupService popupService,
    ILogger<SettingsPageViewModel> logger) : PageViewModelBase
{
    private static readonly CallAudioDevice DefaultInputDevice = new(null, "System default", CallAudioDeviceKind.Input);

    private static readonly CallAudioDevice DefaultOutputDevice =
        new(null, "System default", CallAudioDeviceKind.Output);

    private static readonly HashSet<string> BrowserAliasDeviceIds = ["default", "communications"];

    private bool _applyingDevices;

    [ObservableProperty] public partial CallAudioDevice? SelectedInputDevice { get; set; }

    [ObservableProperty] public partial CallAudioDevice? SelectedOutputDevice { get; set; }

    [ObservableProperty] public partial bool IsLoadingDevices { get; set; }

    [ObservableProperty] public partial bool IsLoadingDeviceSessions { get; set; }

    public ObservableCollection<CallAudioDevice> InputDevices { get; } = [DefaultInputDevice];

    public ObservableCollection<CallAudioDevice> OutputDevices { get; } = [DefaultOutputDevice];

    public ObservableCollection<DeviceSessionContract> DeviceSessions { get; } = [];

    public bool HasOtherDeviceSessions => DeviceSessions.Any(session => !session.IsCurrent);

    public CallSettings CallSettings => appSettings.CallSettings;

    public bool IsOutputDeviceSelectionSupported { get; } = OperatingSystem.IsWindows();

    public IReadOnlyList<AccentColor> AccentColors { get; } = Enum.GetValues<AccentColor>();
    public IReadOnlyList<ThemeMode> ThemeModes { get; } = Enum.GetValues<ThemeMode>();

    public NotificationSettings NotificationSettings => appSettings.NotificationSettings;
    public ThemeSettings ThemeSettings => appSettings.ThemeSettings;
    public string ApplicationVersion => appSettings.AppVersion;
    public string Platform => appSettings.Platform;

    public override Task OnNavigatedToAsync(object? parameter, CancellationToken cancellationToken)
    {
        if (RefreshDevicesCommand.CanExecute(null))
        {
            RefreshDevicesCommand.Execute(null);
        }

        if (RefreshDeviceSessionsCommand.CanExecute(null))
        {
            RefreshDeviceSessionsCommand.Execute(null);
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task RefreshDevicesAsync(CancellationToken cancellationToken)
    {
        IsLoadingDevices = true;
        try
        {
            var devices = await callMediaHost.GetAudioDevicesAsync(cancellationToken);
            ApplyDevices(devices);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            LogDeviceRefreshFailed(exception);
            ApplyDevices([]);
        }
        finally
        {
            IsLoadingDevices = false;
        }
    }

    partial void OnSelectedInputDeviceChanged(CallAudioDevice? value)
    {
        if (!_applyingDevices && value is not null)
        {
            CallSettings.InputDeviceId = value.Id;
        }
    }

    partial void OnSelectedOutputDeviceChanged(CallAudioDevice? value)
    {
        if (!_applyingDevices && value is not null)
        {
            CallSettings.OutputDeviceId = value.Id;
        }
    }

    private void ApplyDevices(IReadOnlyList<CallAudioDevice> devices)
    {
        _applyingDevices = true;
        try
        {
            Replace(InputDevices, DefaultInputDevice, devices, CallAudioDeviceKind.Input);
            Replace(OutputDevices, DefaultOutputDevice, devices, CallAudioDeviceKind.Output);
            SelectedInputDevice = InputDevices.FirstOrDefault(device => device.Id == CallSettings.InputDeviceId)
                                  ?? DefaultInputDevice;
            SelectedOutputDevice = OutputDevices.FirstOrDefault(device => device.Id == CallSettings.OutputDeviceId)
                                   ?? DefaultOutputDevice;
        }
        finally
        {
            _applyingDevices = false;
        }
    }

    private static void Replace(
        ObservableCollection<CallAudioDevice> target,
        CallAudioDevice defaultDevice,
        IReadOnlyList<CallAudioDevice> devices,
        CallAudioDeviceKind kind)
    {
        target.Clear();
        target.Add(defaultDevice);
        foreach (var device in devices.Where(device =>
                     device.Kind == kind
                     && !string.IsNullOrEmpty(device.Id)
                     && !BrowserAliasDeviceIds.Contains(device.Id)))
        {
            target.Add(string.IsNullOrWhiteSpace(device.Label) ? device with { Label = "Unknown device" } : device);
        }
    }

    [RelayCommand]
    private async Task RefreshDeviceSessionsAsync(CancellationToken cancellationToken)
    {
        IsLoadingDeviceSessions = true;
        try
        {
            var result = await deviceSessionApiClient.GetDeviceSessionsAsync(cancellationToken);
            if (result.IsFailure)
            {
                toastService.ShowError(result.Error);
                return;
            }

            ApplyDeviceSessions(result.Value
                .OrderByDescending(session => session.IsCurrent)
                .ThenByDescending(session => session.LastSeenAt)
                .Select(session => session with { LastSeenAt = session.LastSeenAt.ToLocalTime() }));
        }
        finally
        {
            IsLoadingDeviceSessions = false;
        }
    }

    [RelayCommand]
    private async Task RevokeDeviceSessionAsync(DeviceSessionContract session, CancellationToken cancellationToken)
    {
        if (!await popupService.ConfirmAsync(
                "Sign out device",
                $"{session.DeviceName} will be signed out and has to log in again.",
                "Sign out",
                cancellationToken))
        {
            return;
        }

        var result = await deviceSessionApiClient.RevokeDeviceSessionAsync(session.SessionId, cancellationToken);
        if (result.IsFailure)
        {
            toastService.ShowError(result.Error);
            return;
        }

        ApplyDeviceSessions(DeviceSessions.Where(candidate => candidate.SessionId != session.SessionId).ToList());
        toastService.ShowSuccess($"{session.DeviceName} was signed out.");
    }

    [RelayCommand]
    private async Task RevokeOtherDeviceSessionsAsync(CancellationToken cancellationToken)
    {
        if (!await popupService.ConfirmAsync(
                "Sign out all other devices",
                "Every device except this one will be signed out and has to log in again.",
                "Sign out all",
                cancellationToken))
        {
            return;
        }

        var result = await deviceSessionApiClient.RevokeOtherDeviceSessionsAsync(cancellationToken);
        if (result.IsFailure)
        {
            toastService.ShowError(result.Error);
            return;
        }

        ApplyDeviceSessions(DeviceSessions.Where(session => session.IsCurrent).ToList());
        toastService.ShowSuccess("All other devices were signed out.");
    }

    private void ApplyDeviceSessions(IEnumerable<DeviceSessionContract> sessions)
    {
        DeviceSessions.Clear();
        foreach (var session in sessions)
        {
            DeviceSessions.Add(session);
        }

        OnPropertyChanged(nameof(HasOtherDeviceSessions));
    }

    [RelayCommand]
    private async Task Logout(CancellationToken cancellationToken)
    {
        try
        {
            await sessionService.LogoutAsync(cancellationToken);
        }
        finally
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }

    [LoggerMessage(LogLevel.Warning, "Audio devices could not be loaded.")]
    private partial void LogDeviceRefreshFailed(Exception exception);
}