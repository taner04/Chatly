using Chatly.Desktop.Services.Api.Hubs;

namespace Chatly.Desktop.UnitTests.Tests.Services.Hubs.TestDoubles;

internal sealed class TestCallHandler : HubMessageHandler<TestCall>
{
    internal TestCall? Received { get; private set; }

    protected override Task HandleMessageAsync(TestCall message)
    {
        Received = message;
        return Task.CompletedTask;
    }
}
