namespace Chatly.Desktop.Abstraction.Navigation;

public interface INavigableViewModel
{
    Task OnNavigatedToAsync(object? parameter, CancellationToken cancellationToken);

    Task OnNavigatedFromAsync(CancellationToken cancellationToken);
}