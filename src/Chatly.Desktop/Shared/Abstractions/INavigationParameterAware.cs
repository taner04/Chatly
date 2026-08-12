namespace Chatly.Desktop.Shared.Abstractions;

public interface INavigationParameterAware : INavigavablePage
{
    void SetNavigationParameter(object parameter);
}