using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.Services.Navigation;

internal abstract class NavigationState(Type viewModelType, object? parameter)
{
    public Type ViewModelType { get; } = viewModelType;

    public object? Parameter { get; } = parameter;

    public abstract INavigableViewModel ViewModel { get; }

    public abstract void ShowPage(INavigationView navigationView);

    public static NavigationState Create(
        IServiceProvider serviceProvider,
        Type viewModelType,
        object? parameter)
    {
        var viewType = typeof(INavigableView<>).MakeGenericType(viewModelType);
        var stateType = typeof(NavigationState<>).MakeGenericType(viewModelType);
        var view = serviceProvider.GetRequiredService(viewType);

        return (NavigationState)Activator.CreateInstance(stateType, view, parameter)!;
    }
}

internal sealed class NavigationState<T>(INavigableView<T> view, object? parameter = null)
    : NavigationState(typeof(T), parameter) where T : INavigableViewModel
{
    public override INavigableViewModel ViewModel => view.ViewModel;

    public override void ShowPage(INavigationView navigationView)
    {
        navigationView.SetPage(view);
    }
}