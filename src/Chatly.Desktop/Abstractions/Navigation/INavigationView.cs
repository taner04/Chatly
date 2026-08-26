namespace Chatly.Desktop.Abstractions.Navigation;

public interface INavigationView
{
    void SetPage<T>(INavigableView<T> view) where T : INavigableViewModel;
}
