using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Popups;

public abstract partial class PopupOverlayViewModel : ViewModelBase, IPopupViewModel
{
    private readonly TaskCompletionSource _completed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public abstract string Title { get; }

    public Task Completion => _completed.Task;

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
}