using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Navigation;

[SingletonService(typeof(INavigationService))]
public sealed partial class NavigationService(
    IServiceProvider serviceProvider,
    ILogger<NavigationService> logger) : INavigationService
{
    private readonly Stack<NavigationState> _backStack = new();
    private readonly Stack<NavigationState> _forwardStack = new();
    private NavigationState? _currentState;
    private INavigationView? _navigationView;

    public event EventHandler<NavigatedEventArgs>? Navigated;

    public void SetNavigationView(INavigationView navigationView)
    {
        _navigationView = navigationView ?? throw new ArgumentNullException(nameof(navigationView));
    }

    public bool NavigateTo<T>() where T : INavigableViewModel
    {
        ValidateViewModelType(typeof(T));

        if (_currentState?.ViewModelType == typeof(T))
        {
            return false;
        }

        NavigateToNewState(typeof(T), null);
        return true;
    }

    public bool NavigateTo<T>(object parameter) where T : INavigableViewModel
    {
        ValidateViewModelType(typeof(T));
        ArgumentNullException.ThrowIfNull(parameter);

        if (_currentState is { } current && current.ViewModelType == typeof(T))
        {
            _ = NotifyNavigatedToAsync(current.ViewModel, parameter);
            return true;
        }

        NavigateToNewState(typeof(T), parameter);
        return true;
    }

    public bool GoBack()
    {
        return NavigateHistory(_backStack, _forwardStack);
    }

    public bool GoForward()
    {
        return NavigateHistory(_forwardStack, _backStack);
    }

    private void NavigateToNewState(Type viewModelType, object? parameter)
    {
        if (_currentState is not null)
        {
            _backStack.Push(_currentState);
        }

        _forwardStack.Clear();
        Show(NavigationState.Create(serviceProvider, viewModelType, parameter));
    }

    private bool NavigateHistory(Stack<NavigationState> source, Stack<NavigationState> destination)
    {
        if (!source.TryPop(out var target))
        {
            return false;
        }

        if (_currentState is not null)
        {
            destination.Push(_currentState);
        }

        Show(target);
        return true;
    }

    private void Show(NavigationState state)
    {
        var navigationView = _navigationView
                             ?? throw new InvalidOperationException("A navigation view must be set before navigating.");

        _ = NotifyNavigatedFromAsync(_currentState?.ViewModel);

        state.ShowPage(navigationView);
        Navigated?.Invoke(this, new NavigatedEventArgs(state.ViewModelType));

        _currentState = state;

        _ = NotifyNavigatedToAsync(state.ViewModel, state.Parameter);
    }

    private static void ValidateViewModelType(Type viewModelType)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);

        if (!typeof(INavigableViewModel).IsAssignableFrom(viewModelType))
        {
            throw new ArgumentException(
                $"Type '{viewModelType}' must implement {nameof(INavigableViewModel)}.",
                nameof(viewModelType));
        }
    }
}