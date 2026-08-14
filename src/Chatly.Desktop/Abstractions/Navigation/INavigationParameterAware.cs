namespace Chatly.Desktop.Abstractions.Navigation;

public interface INavigationParameterAware : INavigablePage
{
    void SetNavigationParameter(object parameter);
}
