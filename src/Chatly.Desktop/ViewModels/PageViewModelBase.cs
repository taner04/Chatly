namespace Chatly.Desktop.ViewModels;

public abstract class PageViewModelBase : ViewModelBase, INavigableViewModel
{
    public virtual Task OnNavigatedToAsync(
        object? parameter,
        CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public virtual Task OnNavigatedFromAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}