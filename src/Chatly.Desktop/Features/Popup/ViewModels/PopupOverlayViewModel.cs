using Chatly.Desktop.Shared.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace Chatly.Desktop.Features.Popup.ViewModels;

public sealed partial class PopupOverlayViewModel : ViewModelBase
{
    private TaskCompletionSource? _closed;

    [ObservableProperty]
    public partial bool IsOpen { get; private set; }

    [ObservableProperty]
    public partial string? Title { get; private set; }

    [ObservableProperty]
    public partial object? Content { get; private set; }

    [ObservableProperty]
    public partial bool IsCloseButtonVisible { get; private set; } = true;

    public Task ShowAsync(
        string title,
        object content,
        bool isCloseButtonVisible = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(content);

        if (IsOpen)
        {
            throw new InvalidOperationException("A popup is already open.");
        }

        _closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Title = title;
        Content = content;
        IsCloseButtonVisible = isCloseButtonVisible;
        IsOpen = true;

        return _closed.Task;
    }

    [RelayCommand]
    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;
        Title = null;
        Content = null;
        IsCloseButtonVisible = true;

        var closed = _closed;
        _closed = null;
        closed?.TrySetResult();
    }
}
