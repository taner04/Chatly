using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.Models.Settings.Theme;
using Chatly.Desktop.Services.Authentication;
using CommunityToolkit.Mvvm.Input;
using AccentColor = Chatly.Desktop.Models.Settings.Theme.AccentColor;
using ThemeSettings = Chatly.Desktop.Models.Settings.Theme.ThemeSettings;

namespace Chatly.Desktop.ViewModels.Pages;

[SingletonService]
public sealed partial class SettingsPageViewModel : PageViewModelBase
{
    private readonly SessionService _sessionService;

    public SettingsPageViewModel(
        AppSettings appSettings,
        SessionService sessionService)
    {
        _sessionService = sessionService;
        NotificationSettings = appSettings.NotificationSettings;
        ThemeSettings = appSettings.ThemeSettings;
    }

    public IReadOnlyList<AccentColor> AccentColors { get; } = Enum.GetValues<AccentColor>();
    public IReadOnlyList<ThemeMode> ThemeModes { get; } = Enum.GetValues<ThemeMode>();

    public NotificationSettings NotificationSettings { get; }
    public ThemeSettings ThemeSettings { get; }

    public string ApplicationVersion { get; } =
        typeof(SettingsPageViewModel).Assembly.GetName().Version?.ToString(3) ?? "Unknown";

    public string Platform { get; } = OperatingSystem.IsMacOS()
        ? "macOS"
        : OperatingSystem.IsWindows()
            ? "Windows"
            : Environment.OSVersion.Platform.ToString();

    [RelayCommand]
    private async Task Logout(CancellationToken cancellationToken)
    {
        try
        {
            await _sessionService.LogoutAsync(cancellationToken);
        }
        finally
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }
}