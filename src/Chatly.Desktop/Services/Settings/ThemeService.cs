using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.Models.Settings.Theme;

namespace Chatly.Desktop.Services.Settings;

[SingletonService]
internal sealed class ThemeService : IDisposable
{
    private readonly ThemeSettings _settings;

    public ThemeService(AppSettings appSettings)
    {
        _settings = appSettings.ThemeSettings;
        _settings.PropertyChanged += Settings_OnPropertyChanged;
    }

    public void Dispose()
    {
        _settings.PropertyChanged -= Settings_OnPropertyChanged;
    }

    public void Apply()
    {
        var application = Application.Current;
        if (application is null)
        {
            return;
        }

        var accentColor = Color.Parse(_settings.AccentColorHex);

        application.RequestedThemeVariant = _settings.ThemeVariant;
        application.Resources["ChatlyAccentBrush"] = new SolidColorBrush(accentColor);
        application.Resources["ChatlyAccentSubtleBrush"] =
            new SolidColorBrush(Color.FromArgb(0x22, accentColor.R, accentColor.G, accentColor.B));
        application.Resources["ChatlyOnAccentBrush"] =
            new SolidColorBrush(_settings.AccentColor == AccentColor.Yellow ? Colors.Black : Colors.White);
        application.Resources["TabItemHeaderSelectedPipeFill"] = new SolidColorBrush(accentColor);
    }

    private void Settings_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ThemeSettings.ThemeVariant) or nameof(ThemeSettings.AccentColorHex))
        {
            Apply();
        }
    }
}
