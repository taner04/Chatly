namespace Chatly.Desktop.Abstraction.Navigation;

public interface INavigationService
{
    INavigableViewModel? CurrentPage { get; }
    event EventHandler<NavigatedEventArgs>? Navigated;

    Task<bool> NavigateToAsync(Type viewModelType, CancellationToken cancellationToken = default);

    Task<bool> NavigateToAsync(
        Type viewModelType,
        object parameter,
        CancellationToken cancellationToken = default);

    Task<bool> GoBackAsync(CancellationToken cancellationToken = default);

    Task<bool> GoForwardAsync(CancellationToken cancellationToken = default);
}