using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.Models.Settings.Theme;
using Chatly.Desktop.Services.Authentication;
using AccentColor = Chatly.Desktop.Models.Settings.Theme.AccentColor;
using ThemeSettings = Chatly.Desktop.Models.Settings.Theme.ThemeSettings;

namespace Chatly.Desktop.ViewModels.Pages;

[SingletonService]
public sealed partial class SettingsPageViewModel(
    AppSettings appSettings,
    SessionService sessionService) : PageViewModelBase
{
    public IReadOnlyList<AccentColor> AccentColors { get; } = Enum.GetValues<AccentColor>();
    public IReadOnlyList<ThemeMode> ThemeModes { get; } = Enum.GetValues<ThemeMode>();

    public NotificationSettings NotificationSettings => appSettings.NotificationSettings;
    public ThemeSettings ThemeSettings => appSettings.ThemeSettings;
    public string ApplicationVersion => appSettings.ApplicationVersion;
    public string Platform => appSettings.Platform;

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
}