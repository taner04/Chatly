namespace Chatly.Desktop.Abstraction.Navigation;

public interface INavigationService
{
    event EventHandler<NavigatedEventArgs>? Navigated;

    void SetNavigationView(INavigationView navigationView);

    Task<bool> NavigateToAsync<T>(CancellationToken cancellationToken = default)
        where T : INavigableViewModel;

    Task<bool> NavigateToAsync<T>(object parameter, CancellationToken cancellationToken = default)
        where T : INavigableViewModel;

    Task<bool> GoBackAsync(CancellationToken cancellationToken = default);

    Task<bool> GoForwardAsync(CancellationToken cancellationToken = default);
}