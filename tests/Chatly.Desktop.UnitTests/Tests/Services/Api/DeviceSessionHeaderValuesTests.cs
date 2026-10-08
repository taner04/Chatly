using Chatly.Contracts.Common;
using Chatly.Desktop.Services.Api;
using Chatly.Desktop.UnitTests.Tests.Services.Calls.TestDoubles;

namespace Chatly.Desktop.UnitTests.Tests.Services.Api;

public sealed class DeviceSessionHeaderValuesTests
{
    [Fact]
    public void Create_Should_ReturnAllDeviceHeaders_When_DeviceIdExists()
    {
        var appSettings = CallSessionFactory.CreateAppSettings();
        var deviceId = Guid.NewGuid();
        appSettings.DeviceSettings.DeviceId = deviceId;

        var headers = DeviceSessionHeaderValues.Create(appSettings);

        headers.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            [DeviceSessionHeaders.DeviceId] = deviceId.ToString(),
            [DeviceSessionHeaders.DeviceName] = appSettings.DeviceName,
            [DeviceSessionHeaders.Platform] = appSettings.Platform,
            [DeviceSessionHeaders.AppVersion] = appSettings.AppVersion
        });
    }

    [Fact]
    public void Create_Should_ReturnNoHeaders_When_DeviceIdIsMissing()
    {
        var headers = DeviceSessionHeaderValues.Create(CallSessionFactory.CreateAppSettings());

        headers.Should().BeEmpty();
    }
}