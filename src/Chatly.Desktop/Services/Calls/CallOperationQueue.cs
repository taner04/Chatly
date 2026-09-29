namespace Chatly.Desktop.Services.Calls;

internal sealed class CallOperationQueue
{
    private readonly Lock _syncLock = new();
    private Task? _stopTask;
    private int _stopping;
    private Task _tail = Task.CompletedTask;

    internal Task RunAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        lock (_syncLock)
        {
            if (_stopping != 0)
            {
                return Task.CompletedTask;
            }

            return EnqueueCore(operation, cancellationToken);
        }
    }

    internal Task EnqueueAsync(Func<Task> operation)
    {
        lock (_syncLock)
        {
            if (_stopping != 0)
            {
                return Task.CompletedTask;
            }

            return EnqueueCore(operation, CancellationToken.None);
        }
    }

    internal Task StopAsync(Func<Task> finalOperation)
    {
        lock (_syncLock)
        {
            if (_stopTask is not null)
            {
                return _stopTask;
            }

            _stopping = 1;
            _stopTask = EnqueueCore(finalOperation, CancellationToken.None);
            return _stopTask;
        }
    }

    private Task EnqueueCore(Func<Task> operation, CancellationToken cancellationToken)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _tail = RunAfterAsync(_tail, operation, cancellationToken, completion);
        return completion.Task;
    }

    private static async Task RunAfterAsync(
        Task previous,
        Func<Task> operation,
        CancellationToken cancellationToken,
        TaskCompletionSource completion)
    {
        await using var cancellationRegistration =
            cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken));

        try
        {
            await previous;
        }
        catch
        {
            // A failed operation must not prevent subsequent cleanup or shutdown.
        }

        await Task.Yield();
        if (cancellationToken.IsCancellationRequested)
        {
            completion.TrySetCanceled(cancellationToken);
            return;
        }

        cancellationRegistration.Unregister();
        if (cancellationToken.IsCancellationRequested)
        {
            completion.TrySetCanceled(cancellationToken);
            return;
        }

        try
        {
            await operation();
            completion.TrySetResult();
        }
        catch (OperationCanceledException exception)
        {
            completion.TrySetCanceled(exception.CancellationToken);
        }
        catch (Exception exception)
        {
            completion.TrySetException(exception);
        }
    }
}