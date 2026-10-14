namespace Chatly.Desktop.ViewModels;

public abstract class PageViewModelBase : ViewModelBase, INavigatedTo, INavigatedFrom
{
    public virtual Task OnNavigatedToAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task OnNavigatedFromAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
