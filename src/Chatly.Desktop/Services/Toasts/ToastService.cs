using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.Services.Toasts;

[SingletonService(typeof(IToastService))]
internal sealed class ToastService : IToastService
{
    private const int MaxToastDurationMs = 5000;
    private const int MaxToasts = 5;
    private readonly LinkedList<Guid> _toastOrder = [];

    private readonly Dictionary<Guid, ActiveToast> _toasts = [];
    private IToastHost? _toastHost;

    public void AddToast(IToastViewModel toastViewModel)
    {
        ArgumentNullException.ThrowIfNull(toastViewModel);
        UiThreadDispatcher.SafeInvoke(() => AddToastCore(toastViewModel));
    }

    public void RemoveToast(Guid id)
    {
        UiThreadDispatcher.SafeInvoke(() => RemoveToastCore(id));
    }

    public void SetToastHost(IToastHost toastHost)
    {
        ArgumentNullException.ThrowIfNull(toastHost);
        _toastHost = toastHost;
    }

    private IToastHost GetToastHost()
    {
        return _toastHost ??
               throw new InvalidOperationException(
                   "Toast host is not set. Please set the toast host before adding or removing toasts.");
    }

    private void AddToastCore(IToastViewModel toastViewModel)
    {
        if (_toasts.ContainsKey(toastViewModel.Id))
        {
            RemoveToastCore(toastViewModel.Id);
        }

        if (_toastOrder.Count >= MaxToasts && _toastOrder.Last is { } oldestToast)
        {
            RemoveToastCore(oldestToast.Value);
        }

        var cancellationTokenSource = new CancellationTokenSource();
        var orderNode = _toastOrder.AddFirst(toastViewModel.Id);
        _toasts.Add(toastViewModel.Id, new ActiveToast(toastViewModel, cancellationTokenSource, orderNode));
        toastViewModel.Dismissed += OnToastDismissed;
        GetToastHost().AddToast(toastViewModel);
        _ = AutoDismissAsync(toastViewModel.Id, cancellationTokenSource);
    }

    private async Task AutoDismissAsync(Guid id, CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            await Task.Delay(MaxToastDurationMs, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        UiThreadDispatcher.SafeInvoke(() => RemoveToastCore(id, cancellationTokenSource));
    }

    private void OnToastDismissed(object? sender, EventArgs e)
    {
        if (sender is IToastViewModel toastViewModel)
        {
            RemoveToast(toastViewModel.Id);
        }
    }

    private void RemoveToastCore(Guid id, CancellationTokenSource? expectedCancellationTokenSource = null)
    {
        if (!_toasts.TryGetValue(id, out var toast) ||
            (expectedCancellationTokenSource is not null &&
             !ReferenceEquals(toast.CancellationTokenSource, expectedCancellationTokenSource)))
        {
            return;
        }

        _toasts.Remove(id);
        _toastOrder.Remove(toast.OrderNode);
        toast.ViewModel.Dismissed -= OnToastDismissed;
        toast.CancellationTokenSource.Cancel();
        toast.CancellationTokenSource.Dispose();
        GetToastHost().RemoveToast(id);
    }

    private sealed record ActiveToast(
        IToastViewModel ViewModel,
        CancellationTokenSource CancellationTokenSource,
        LinkedListNode<Guid> OrderNode);
}
