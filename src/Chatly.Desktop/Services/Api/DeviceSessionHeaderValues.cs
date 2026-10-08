using Chatly.Desktop.Models.Settings;

namespace Chatly.Desktop.Services.Api;

internal static class DeviceSessionHeaderValues
{
    internal static IReadOnlyDictionary<string, string> Create(AppSettings appSettings) =>
        appSettings.DeviceSettings.DeviceId is { } deviceId
            ? new Dictionary<string, string>
            {
                [DeviceSessionHeaders.DeviceId] = deviceId.ToString(),
                [DeviceSessionHeaders.DeviceName] = appSettings.DeviceName,
                [DeviceSessionHeaders.Platform] = appSettings.Platform,
                [DeviceSessionHeaders.AppVersion] = appSettings.AppVersion
            }
            : new Dictionary<string, string>();
}