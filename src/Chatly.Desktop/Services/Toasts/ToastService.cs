using Chatly.Desktop.Abstractions.Toasts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Chatly.Desktop.Services.Toasts;

internal sealed class ToastService : IToastService
{
    private const int MAX_TOAST_DURATION_MS = 5000;
    private const int MAX_TOASTS = 5;
    private IToastHost? _toastHost;

    private readonly Dictionary<Guid, ActiveToast> _toasts = [];
    private readonly LinkedList<Guid> _toastOrder = [];

    public void AddToast(IToastViewModel toastViewModel)
    {
        ArgumentNullException.ThrowIfNull(toastViewModel);

        if (Dispatcher.UIThread.CheckAccess())
        {
            AddToastCore(toastViewModel);
            return;
        }

        Dispatcher.UIThread.Post(() => AddToastCore(toastViewModel));
    }

    public void RemoveToast(Guid id)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            RemoveToastCore(id);
            return;
        }

        Dispatcher.UIThread.Post(() => RemoveToastCore(id));
    }

    public void SetToastHost(IToastHost toastHost)
    {
        ArgumentNullException.ThrowIfNull(toastHost);
        _toastHost = toastHost;
    }

    private IToastHost GetToastHost()
    {
        return _toastHost ?? throw new InvalidOperationException("Toast host is not set. Please set the toast host before adding or removing toasts.");
    }

    private void AddToastCore(IToastViewModel toastViewModel)
    {
        if (_toasts.ContainsKey(toastViewModel.Id))
        {
            RemoveToastCore(toastViewModel.Id);
        }

        if (_toastOrder.Count >= MAX_TOASTS && _toastOrder.Last is { } oldestToast)
        {
            RemoveToastCore(oldestToast.Value);
        }

        var cancellationTokenSource = new CancellationTokenSource();
        var orderNode = _toastOrder.AddFirst(toastViewModel.Id);
        _toasts.Add(toastViewModel.Id, new ActiveToast(toastViewModel, cancellationTokenSource, orderNode));
        toastViewModel.ButtonClicked += OnToastButtonClicked;
        GetToastHost().AddToast(toastViewModel);
        _ = AutoDismissAsync(toastViewModel.Id, cancellationTokenSource);
    }

    private async Task AutoDismissAsync(Guid id, CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            await Task.Delay(MAX_TOAST_DURATION_MS, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        Dispatcher.UIThread.Post(() => RemoveToastCore(id, cancellationTokenSource));
    }

    private void OnToastButtonClicked(object? sender, ToastButtonClickedEventArgs e)
    {
        if (sender is IToastViewModel toastViewModel)
        {
            RemoveToast(toastViewModel.Id);
        }
    }

    private void RemoveToastCore(Guid id, CancellationTokenSource? expectedCancellationTokenSource = null)
    {
        if (!_toasts.TryGetValue(id, out var toast) ||
            expectedCancellationTokenSource is not null &&
            !ReferenceEquals(toast.CancellationTokenSource, expectedCancellationTokenSource))
        {
            return;
        }

        _toasts.Remove(id);
        _toastOrder.Remove(toast.OrderNode);
        toast.ViewModel.ButtonClicked -= OnToastButtonClicked;
        toast.CancellationTokenSource.Cancel();
        toast.CancellationTokenSource.Dispose();
        GetToastHost().RemoveToast(id);
    }

    private sealed record ActiveToast(
        IToastViewModel ViewModel,
        CancellationTokenSource CancellationTokenSource,
        LinkedListNode<Guid> OrderNode);
}
