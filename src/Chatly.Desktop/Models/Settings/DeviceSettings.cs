using Chatly.Desktop.Abstraction.Settings;

namespace Chatly.Desktop.Models.Settings;

public sealed class DeviceSettings : ISettingsGroup
{
    public Guid? DeviceId { get; set; }

    public static string GroupName => "Device";
}