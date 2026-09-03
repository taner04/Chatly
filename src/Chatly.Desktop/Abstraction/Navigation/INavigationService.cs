namespace Chatly.Desktop.Abstraction.Navigation;

public interface INavigationService
{
    event EventHandler<NavigatedEventArgs>? Navigated;

    void SetNavigationView(INavigationView navigationView);

    bool NavigateTo<T>() where T : INavigableViewModel;

    bool NavigateTo<T>(object parameter) where T : INavigableViewModel;

    bool GoBack();

    bool GoForward();
}