namespace Chatly.Desktop.Extensions;

internal static class NavigationExtensions
{
    extension(INavigationService navigationService)
    {
        public Task<bool> NavigateToAsync<T>(CancellationToken cancellationToken = default)
            where T : INavigableViewModel =>
            navigationService.NavigateToAsync(typeof(T), cancellationToken);

        public Task<bool> NavigateToAsync<T>(object parameter, CancellationToken cancellationToken = default)
            where T : INavigableViewModel =>
            navigationService.NavigateToAsync(typeof(T), parameter, cancellationToken);
    }
}