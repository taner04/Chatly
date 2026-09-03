namespace Chatly.Desktop.Abstraction.Navigation;

public interface INavigationParameterAware : INavigableViewModel
{
    Task OnNavigatedToAsync(object parameter);

    Task OnNavigatedFromAsync();
}