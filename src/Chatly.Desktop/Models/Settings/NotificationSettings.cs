using Chatly.Desktop.Abstraction.Settings;

namespace Chatly.Desktop.Models.Settings;

public sealed partial class NotificationSettings : ObservableObject, ISettingsGroup
{
    [ObservableProperty] public partial bool PlaySound { get; set; } = true;
    [ObservableProperty] public partial bool PlayRingtone { get; set; } = true;
    public static string GroupName => "Notification";
}