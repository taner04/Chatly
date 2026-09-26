using Chatly.WebApi.Common.Infrastructure;
using Chatly.WebApi.Features.Users.Models;

namespace Chatly.WebApi.UnitTests.Tests.Common.Infrastructure;

public sealed class OnlinePresenceTrackerTests
{
    private readonly OnlinePresenceTracker _tracker = new();
    private readonly UserId _userId = UserId.From(Guid.NewGuid());

    [Fact]
    public void Connect_Should_ReportComingOnlineOnlyOnce_When_UserOpensSeveralConnections()
    {
        _tracker.Connect(_userId, "phone").Should().BeTrue();
        _tracker.Connect(_userId, "laptop").Should().BeFalse();

        _tracker.IsOnline(_userId).Should().BeTrue();
    }

    [Fact]
    public void TryBeginOffline_Should_KeepUserOnline_When_OtherConnectionsRemain()
    {
        _tracker.Connect(_userId, "phone");
        _tracker.Connect(_userId, "laptop");

        _tracker.TryBeginOffline(_userId, "phone", out _).Should().BeFalse();

        _tracker.IsOnline(_userId).Should().BeTrue();
    }

    [Fact]
    public void TryCompleteOffline_Should_SetUserOffline_When_GracePeriodEndsWithoutReconnect()
    {
        _tracker.Connect(_userId, "phone");

        _tracker.TryBeginOffline(_userId, "phone", out var offlineVersion).Should().BeTrue();
        _tracker.IsOnline(_userId).Should().BeTrue();

        _tracker.TryCompleteOffline(_userId, offlineVersion).Should().BeTrue();
        _tracker.IsOnline(_userId).Should().BeFalse();
    }

    [Fact]
    public void Connect_Should_CancelPendingOffline_When_UserReconnectsDuringGracePeriod()
    {
        _tracker.Connect(_userId, "phone");
        _tracker.TryBeginOffline(_userId, "phone", out var offlineVersion);

        _tracker.Connect(_userId, "phone-reconnected").Should().BeFalse();

        _tracker.TryCompleteOffline(_userId, offlineVersion).Should().BeFalse();
        _tracker.IsOnline(_userId).Should().BeTrue();
    }

    [Fact]
    public void TryCompleteOffline_Should_IgnoreStaleVersion_When_UserDisconnectedAgainLater()
    {
        _tracker.Connect(_userId, "first");
        _tracker.TryBeginOffline(_userId, "first", out var staleVersion);
        _tracker.Connect(_userId, "second");
        _tracker.TryBeginOffline(_userId, "second", out var currentVersion);

        _tracker.TryCompleteOffline(_userId, staleVersion).Should().BeFalse();
        _tracker.IsOnline(_userId).Should().BeTrue();
        _tracker.TryCompleteOffline(_userId, currentVersion).Should().BeTrue();
        _tracker.IsOnline(_userId).Should().BeFalse();
    }
}
