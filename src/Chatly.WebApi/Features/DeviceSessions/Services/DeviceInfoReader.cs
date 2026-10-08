using Chatly.WebApi.Features.DeviceSessions.Exceptions;
using Chatly.WebApi.Features.DeviceSessions.Models;

namespace Chatly.WebApi.Features.DeviceSessions.Services;

internal static class DeviceInfoReader
{
    internal static DeviceInfo Read(IHeaderDictionary headers)
    {
        if (headers[DeviceSessionHeaders.DeviceId] is not { Count: 1 } deviceIdValue
            || !Guid.TryParse(deviceIdValue, out var deviceId)
            || deviceId == Guid.Empty)
        {
            throw new DeviceSessionHeaderInvalidException(DeviceSessionHeaders.DeviceId);
        }

        return new DeviceInfo(
            deviceId,
            ReadHeader(headers, DeviceSessionHeaders.DeviceName, DeviceSession.MaxDeviceNameLength),
            ReadHeader(headers, DeviceSessionHeaders.Platform, DeviceSession.MaxPlatformLength),
            ReadHeader(headers, DeviceSessionHeaders.AppVersion, DeviceSession.MaxAppVersionLength));
    }

    private static string ReadHeader(IHeaderDictionary headers, string name, int maxLength) =>
        headers[name] is { Count: 1 } values
        && values.ToString().Trim() is { Length: > 0 } value
        && value.Length <= maxLength
            ? value
            : throw new DeviceSessionHeaderInvalidException(name);
}