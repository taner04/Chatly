namespace Chatly.Desktop.Abstraction.Navigation;

public interface INavigationAware : INavigableViewModel
{
    Task OnNavigatedToAsync();

    Task OnNavigatedFromAsync();
}