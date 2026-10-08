using Chatly.Contracts.Features.Hubs.Abstraction;
using Chatly.Desktop.Abstraction.Settings;
using Chatly.Desktop.Models.Settings;
using Chatly.Desktop.Models.Settings.Theme;
using Chatly.Desktop.Services.Calls;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chatly.Desktop.UnitTests.Tests.Services.Calls.TestDoubles;

internal static class CallSessionFactory
{
    internal static CallSession Create(
        ICallingHubServer hub,
        FakeCallMediaHost media,
        FakeNotificationSoundPlayer soundPlayer,
        AppSettings? appSettings = null) =>
        new(
            hub,
            media,
            new CallToneController(soundPlayer, NullLogger<CallToneController>.Instance),
            appSettings ?? CreateAppSettings(),
            NullLogger<CallSession>.Instance);

    internal static AppSettings CreateAppSettings()
    {
        var store = Substitute.For<ISettingsStore>();
        store.LoadSettings<NotificationSettings>().Returns(new NotificationSettings());
        store.LoadSettings<ThemeSettings>().Returns(new ThemeSettings());
        store.LoadSettings<CallSettings>().Returns(new CallSettings());
        store.LoadSettings<DeviceSettings>().Returns(new DeviceSettings());
        return new AppSettings(store);
    }
}