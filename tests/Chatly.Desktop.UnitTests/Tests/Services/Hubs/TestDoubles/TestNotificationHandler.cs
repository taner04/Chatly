using Chatly.Desktop.Services.Api.Hubs;

namespace Chatly.Desktop.UnitTests.Tests.Services.Hubs.TestDoubles;

internal sealed class TestNotificationHandler : HubMessageHandler<TestNotification>
{
    internal TestNotification? Received { get; private set; }

    protected override Task HandleMessageAsync(TestNotification message)
    {
        Received = message;
        return Task.CompletedTask;
    }
}