using System.Net.Http.Headers;

namespace Chatly.WebApi.IntegrationTests.Infrastructure.Mocks.DeviceSession;

public static class DeviceSessionHeadersMock
{
    private const string DeviceName = "test-device";
    private const string Platform = "tests";
    private const string AppVersion = "1.0.0";

    public static void Apply(HttpRequestHeaders headers, Guid deviceId)
    {
        foreach (var (name, value) in Create(deviceId))
        {
            headers.Add(name, value);
        }
    }

    public static IReadOnlyDictionary<string, string> Create(Guid deviceId) =>
        new Dictionary<string, string>
        {
            [DeviceSessionHeaders.DeviceId] = deviceId.ToString(),
            [DeviceSessionHeaders.DeviceName] = DeviceName,
            [DeviceSessionHeaders.Platform] = Platform,
            [DeviceSessionHeaders.AppVersion] = AppVersion
        };
}