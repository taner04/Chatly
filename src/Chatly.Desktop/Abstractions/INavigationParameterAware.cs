namespace Chatly.Desktop.Abstractions;

public interface INavigationParameterAware : INavigavablePage
{
    void SetNavigationParameter(object parameter);
}