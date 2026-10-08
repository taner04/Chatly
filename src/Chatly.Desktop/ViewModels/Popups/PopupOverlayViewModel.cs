namespace Chatly.Desktop.ViewModels.Popups;

public abstract class PopupOverlayViewModel : ViewModelBase, IPopupViewModel
{
    private readonly TaskCompletionSource _completed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public abstract string Title { get; }

    public virtual bool IsDismissible => false;

    public virtual double PopupWidth => 540;

    public virtual double PopupMinHeight => 300;

    public Task Completion => _completed.Task;

    public virtual void CloseOverlay()
    {
        _completed.TrySetResult();
    }
}