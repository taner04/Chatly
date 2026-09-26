using Chatly.Desktop.Abstraction.Hubs;
using Chatly.Desktop.Services.Api.Hubs;
using Chatly.Desktop.UnitTests.Tests.Services.Hubs.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chatly.Desktop.UnitTests.Tests.Services.Hubs;

public sealed class HubMessageDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_Should_RouteEachMessageToItsHandler_When_HandlersAreRegistered()
    {
        var callHandler = new TestCallHandler();
        var notificationHandler = new TestNotificationHandler();
        IHubMessageHandler[] handlers = [callHandler, notificationHandler];
        var dispatcher = new HubMessageDispatcher(
            handlers,
            NullLogger<HubMessageDispatcher>.Instance);
        var call = new TestCall(Guid.NewGuid());
        var notification = new TestNotification();

        await dispatcher.DispatchAsync(call);
        await dispatcher.DispatchAsync(notification);

        callHandler.Received.Should().BeSameAs(call);
        notificationHandler.Received.Should().BeSameAs(notification);
    }

    [Fact]
    public async Task DispatchAsync_Should_IgnoreMessage_When_NoHandlerIsRegistered()
    {
        var dispatcher = new HubMessageDispatcher([], NullLogger<HubMessageDispatcher>.Instance);

        var dispatch = () => dispatcher.DispatchAsync(new TestNotification());

        await dispatch.Should().NotThrowAsync();
    }
}
