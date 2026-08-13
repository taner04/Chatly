namespace Chatly.Desktop.Shared.Abstractions;

public interface INavigationParameterAware : INavigablePage
{
    void SetNavigationParameter(object parameter);
}
