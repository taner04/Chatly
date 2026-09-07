using Chatly.Desktop.Abstraction.Settings;

namespace Chatly.Desktop.Models.Settings;

public sealed partial class NotificationSettings : ObservableObject, ISettingsGroup
{
    [ObservableProperty] public partial bool PlaySound { get; set; }
    public static string GroupName => "Notification";
}