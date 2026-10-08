using Chatly.Desktop.Services.Api.Hubs;
using Chatly.Desktop.UnitTests.Infrastructure;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chatly.Desktop.UnitTests.Tests.Services.Hubs;

public sealed class IndefiniteRetryPolicyTests : TestBase
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 2)]
    [InlineData(2, 10)]
    [InlineData(3, 30)]
    [InlineData(1_000, 30)]
    public void NextRetryDelay_Should_NeverGiveUp_When_ReconnectKeepsFailing(int previousRetryCount, int seconds)
    {
        var delay = new IndefiniteRetryPolicy().NextRetryDelay(new RetryContext
        {
            PreviousRetryCount = previousRetryCount
        });

        delay.Should().Be(TimeSpan.FromSeconds(seconds));
    }
}