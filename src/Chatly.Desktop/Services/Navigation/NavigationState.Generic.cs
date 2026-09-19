namespace Chatly.Desktop.Services.Navigation;

internal sealed class NavigationState<T>(INavigableView<T> view, object? parameter = null)
    : NavigationState(typeof(T), parameter) where T : INavigableViewModel
{
    public override INavigableViewModel ViewModel => view.ViewModel;

    public override void ShowPage(INavigationView navigationView)
    {
        navigationView.SetPage(view);
    }
}