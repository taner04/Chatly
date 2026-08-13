namespace Chatly.Desktop.Shared.Abstractions;

public interface INavigationAware : INavigablePage
{
    void OnNavigatedTo();

    void OnNavigatedFrom();
}
