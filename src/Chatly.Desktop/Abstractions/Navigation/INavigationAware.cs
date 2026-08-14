namespace Chatly.Desktop.Abstractions.Navigation;

public interface INavigationAware : INavigablePage
{
    void OnNavigatedTo();

    void OnNavigatedFrom();
}
