using System;
using System.Threading.Tasks;
using Chatly.Desktop.Abstractions.Popups;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Popups;

public abstract partial class PopupOverlayViewModel : ViewModelBase, IPopupViewModel
{
    private TaskCompletionSource _completed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public abstract string Title { get; }

    public Task Completion => _completed.Task;

    [RelayCommand]
    private void Close()
    {
        CloseOverlay();
    }

    [RelayCommand]
    private void Cancel()
    {
        CancelOverlay();
    }

    [RelayCommand]
    private void Fail(Exception exception)
    {
        FailOverlay(exception);
    }

    public virtual void CloseOverlay()
    {
        _completed.TrySetResult();
    }

    public virtual void CancelOverlay()
    {
        _completed.TrySetCanceled();
    }

    public virtual void FailOverlay(Exception exception)
    {
        _completed.TrySetException(exception);
    }
}