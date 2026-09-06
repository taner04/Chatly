using System.Text.Json.Serialization;
using Avalonia.Styling;
using Chatly.Desktop.Abstraction.Settings;
using Chatly.Desktop.Extensions;

namespace Chatly.Desktop.Models.Settings.Theme;

public sealed partial class ThemeSettings : ObservableObject, ISettingsGroup
{
    [ObservableProperty] public partial ThemeMode Mode { get; set; } = ThemeMode.System;
    [ObservableProperty] public partial AccentColor AccentColor { get; set; } = AccentColor.Blue;

    [JsonIgnore] public string AccentColorHex => AccentColor.GetHexCode();

    [JsonIgnore]
    public ThemeVariant ThemeVariant => Mode switch
    {
        ThemeMode.Light => ThemeVariant.Light,
        ThemeMode.Dark => ThemeVariant.Dark,
        _ => ThemeVariant.Default
    };

    public static string GroupName => "Theme";

    partial void OnAccentColorChanged(AccentColor value)
    {
        OnPropertyChanged(nameof(AccentColorHex));
    }

    partial void OnModeChanged(ThemeMode value)
    {
        OnPropertyChanged(nameof(ThemeVariant));
    }
}