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

}
