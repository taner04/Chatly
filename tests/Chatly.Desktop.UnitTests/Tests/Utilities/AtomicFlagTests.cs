using Chatly.Desktop.UnitTests.Infrastructure;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.UnitTests.Tests.Utilities;

public sealed class AtomicFlagTests : TestBase
{
    [Fact]
    public void TrySet_Should_SucceedOnlyOnce_When_FlagIsAlreadySet()
    {
        var flag = new AtomicFlag();

        flag.TrySet().Should().BeTrue();
        flag.TrySet().Should().BeFalse();
    }

    [Fact]
    public void TryReset_Should_SucceedOnlyWhenSet_When_CalledRepeatedly()
    {
        var flag = new AtomicFlag();

        flag.TryReset().Should().BeFalse();
        flag.TrySet();
        flag.TryReset().Should().BeTrue();
        flag.TryReset().Should().BeFalse();
        flag.TrySet().Should().BeTrue();
    }

    [Fact]
    public async Task TrySet_Should_LetExactlyOneCallerWin_When_CalledConcurrently()
    {
        var flag = new AtomicFlag();
        using var start = new ManualResetEventSlim();

        var results = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(() =>
        {
            start.Wait(CurrentCancellationToken);
            return flag.TrySet();
        }, CurrentCancellationToken)).Append(Task.Run(() =>
        {
            start.Set();
            return false;
        }, CurrentCancellationToken)));

        results.Count(result => result).Should().Be(1);
    }
}