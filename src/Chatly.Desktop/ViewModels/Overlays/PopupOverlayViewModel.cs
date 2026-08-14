using System;
using System.Threading.Tasks;
using Chatly.Desktop.Abstractions.Overlays;

namespace Chatly.Desktop.ViewModels.Overlays;

public abstract class PopupOverlayViewModel : ViewModelBase, IPopupViewModel
{
    private TaskCompletionSource _completed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public Task Completion => _completed.Task;

    protected void CloseOverlay() => _completed.TrySetResult();
    protected void CancelOverlay() => _completed.TrySetCanceled();
    protected void FailOverlay(Exception exception) => _completed.TrySetException(exception);

    protected void ResetCompletion()
    {
        _completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}

public abstract class PopupOverlayViewModel<TResult> : ViewModelBase, IPopupViewModel<TResult>
{
    private TaskCompletionSource<TResult> _completed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public Task<TResult> Completion => _completed.Task;
    
    protected void CloseOverlay(TResult result) => _completed.TrySetResult(result);
    protected void CancelOverlay() => _completed.TrySetCanceled();
    protected void FailOverlay(Exception exception) => _completed.TrySetException(exception);

    protected void ResetCompletion()
    {
        _completed = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
