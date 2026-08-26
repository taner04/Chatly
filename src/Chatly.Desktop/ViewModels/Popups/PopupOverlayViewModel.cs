using System;
using System.Threading.Tasks;
using Chatly.Desktop.Abstractions.Popups;

namespace Chatly.Desktop.ViewModels.Popups;

public abstract class PopupOverlayViewModel : ViewModelBase, IPopupViewModel
{
    private TaskCompletionSource _completed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public abstract string Title { get; }

    public Task Completion => _completed.Task;

    public void CloseOverlay()
    {
        _completed.TrySetResult();
    }

    public void CancelOverlay()
    {
        _completed.TrySetCanceled();
    }

    public void FailOverlay(Exception exception)
    {
        _completed.TrySetException(exception);
    }

    protected void ResetCompletion()
    {
        _completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}