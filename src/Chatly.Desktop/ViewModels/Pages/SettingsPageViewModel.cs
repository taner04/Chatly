using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Chatly.Desktop.Abstraction.Calls;
using Chatly.Desktop.Models.Calls;
using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.Models.Settings.Theme;
using Chatly.Desktop.Services.Authentication;
using AccentColor = Chatly.Desktop.Models.Settings.Theme.AccentColor;
using ThemeSettings = Chatly.Desktop.Models.Settings.Theme.ThemeSettings;

namespace Chatly.Desktop.ViewModels.Pages;

[SingletonService]
public sealed partial class SettingsPageViewModel(
    AppSettings appSettings,
    SessionService sessionService,
    ICallMediaHost callMediaHost,
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

    public ObservableCollection<CallAudioDevice> InputDevices { get; } = [DefaultInputDevice];

    public ObservableCollection<CallAudioDevice> OutputDevices { get; } = [DefaultOutputDevice];

    public CallSettings CallSettings => appSettings.CallSettings;

    public bool IsOutputDeviceSelectionSupported { get; } = OperatingSystem.IsWindows();

    public IReadOnlyList<AccentColor> AccentColors { get; } = Enum.GetValues<AccentColor>();
    public IReadOnlyList<ThemeMode> ThemeModes { get; } = Enum.GetValues<ThemeMode>();

    public NotificationSettings NotificationSettings => appSettings.NotificationSettings;
    public ThemeSettings ThemeSettings => appSettings.ThemeSettings;
    public string ApplicationVersion => appSettings.ApplicationVersion;
    public string Platform => appSettings.Platform;

    public override Task OnNavigatedToAsync(object? parameter, CancellationToken cancellationToken)
    {
        if (RefreshDevicesCommand.CanExecute(null))
        {
            RefreshDevicesCommand.Execute(null);
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