using Chatly.Desktop.Services.Calls;

namespace Chatly.Desktop.UnitTests.Tests.Services.Calls;

public sealed class CallOperationQueueTests
{
    [Fact]
    public async Task Queue_Should_RunInOrderAndDropNewWork_When_StopIsRequested()
    {
        var queue = new CallOperationQueue();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var operations = new List<string>();
        var first = queue.EnqueueAsync(async () =>
        {
            operations.Add("first-started");
            started.SetResult();
            await release.Task;
            operations.Add("first-completed");
        });
        await started.Task;

        var second = queue.EnqueueAsync(() =>
        {
            operations.Add("second");
            return Task.CompletedTask;
        });
        var stop = queue.StopAsync(() =>
        {
            operations.Add("stopped");
            return Task.CompletedTask;
        });
        var rejected = queue.EnqueueAsync(() =>
        {
            operations.Add("rejected");
            return Task.CompletedTask;
        });

        release.SetResult();
        await Task.WhenAll(first, second, stop, rejected);

        operations.Should().Equal("first-started", "first-completed", "second", "stopped");
    }

    [Fact]
    public async Task RunAsync_Should_CancelWithoutRunning_When_CanceledWhileWaiting()
    {
        var queue = new CallOperationQueue();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var first = queue.EnqueueAsync(async () =>
        {
            started.SetResult();
            await release.Task;
        });
        await started.Task;
        using var cancellation = new CancellationTokenSource();
        var invoked = false;
        var canceled = queue.RunAsync(
            () =>
            {
                invoked = true;
                return Task.CompletedTask;
            },
            cancellation.Token);

        await cancellation.CancelAsync();

        await FluentActions.Awaiting(() => canceled).Should().ThrowAsync<OperationCanceledException>();
        invoked.Should().BeFalse();

        release.SetResult();
        await first;
        await queue.StopAsync(() => Task.CompletedTask);
        invoked.Should().BeFalse();
    }
}