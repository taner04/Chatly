namespace Chatly.Desktop.Abstraction.Navigation;

public interface INavigationView
{
    void SetPage<T>(INavigableView<T> view) where T : INavigableViewModel;
}