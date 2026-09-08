using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Navigation;

[SingletonService(typeof(INavigationService))]
internal sealed partial class NavigationService(
    IServiceProvider serviceProvider,
    ILogger<NavigationService> logger) : INavigationService
{
    private readonly Stack<NavigationState> _backStack = new();
    private readonly Stack<NavigationState> _forwardStack = new();
    private readonly SemaphoreSlim _transitionLock = new(1, 1);
    private NavigationState? _currentState;
    private INavigationView? _navigationView;

    public event EventHandler<NavigatedEventArgs>? Navigated;

    public void SetNavigationView(INavigationView navigationView)
    {
        _navigationView = navigationView ?? throw new ArgumentNullException(nameof(navigationView));
    }

    public Task<bool> NavigateToAsync<T>(CancellationToken cancellationToken = default)
        where T : INavigableViewModel
    {
        return NavigateToAsync(typeof(T), null, cancellationToken);
    }

    public Task<bool> NavigateToAsync<T>(
        object parameter,
        CancellationToken cancellationToken = default)
        where T : INavigableViewModel
    {
        ArgumentNullException.ThrowIfNull(parameter);
        return NavigateToAsync(typeof(T), parameter, cancellationToken);
    }

    public Task<bool> GoBackAsync(CancellationToken cancellationToken = default)
    {
        return NavigateHistoryAsync(_backStack, _forwardStack, cancellationToken);
    }

    public Task<bool> GoForwardAsync(CancellationToken cancellationToken = default)
    {
        return NavigateHistoryAsync(_forwardStack, _backStack, cancellationToken);
    }

    private async Task<bool> NavigateToAsync(
        Type viewModelType,
        object? parameter,
        CancellationToken cancellationToken)
    {
        ValidateViewModelType(viewModelType);

        await _transitionLock.WaitAsync(cancellationToken);
        try
        {
            if (_currentState is { } current && current.ViewModelType == viewModelType)
            {
                if (parameter is null || Equals(current.Parameter, parameter))
                {
                    return false;
                }

                var replacement = NavigationState.Create(serviceProvider, viewModelType, parameter);
                return await TransitionAsync(replacement, cancellationToken);
            }

            var target = NavigationState.Create(serviceProvider, viewModelType, parameter);
            var previous = _currentState;
            if (!await TransitionAsync(target, cancellationToken))
            {
                return false;
            }

            if (previous is not null)
            {
                _backStack.Push(previous);
            }

            _forwardStack.Clear();
            return true;
        }
        finally
        {
            _transitionLock.Release();
        }
    }

    private async Task<bool> NavigateHistoryAsync(
        Stack<NavigationState> source,
        Stack<NavigationState> destination,
        CancellationToken cancellationToken)
    {
        await _transitionLock.WaitAsync(cancellationToken);
        try
        {
            if (!source.TryPeek(out var target))
            {
                return false;
            }

            var previous = _currentState;
            if (!await TransitionAsync(target, cancellationToken))
            {
                return false;
            }

            source.Pop();
            if (previous is not null)
            {
                destination.Push(previous);
            }

            return true;
        }
        finally
        {
            _transitionLock.Release();
        }
    }

    private async Task<bool> TransitionAsync(
        NavigationState target,
        CancellationToken cancellationToken)
    {
        var navigationView = _navigationView
                             ?? throw new InvalidOperationException(
                                 "A navigation view must be set before navigating.");
        var previous = _currentState;

        try
        {
            if (previous is not null)
            {
                await previous.ViewModel.OnNavigatedFromAsync(cancellationToken);
            }

            await target.ViewModel.OnNavigatedToAsync(target.Parameter, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await RestoreAsync(previous);
            throw;
        }
        catch (Exception exception)
        {
            LogNavigationFailed(target.ViewModelType, exception);
            await RestoreAsync(previous);
            return false;
        }

        target.ShowPage(navigationView);
        _currentState = target;
        Navigated?.Invoke(this, new NavigatedEventArgs(target.ViewModelType));
        return true;
    }

    private async Task RestoreAsync(NavigationState? state)
    {
        if (state is null)
        {
            return;
        }

        try
        {
            await state.ViewModel.OnNavigatedToAsync(state.Parameter, CancellationToken.None);
        }
        catch (Exception exception)
        {
            LogNavigationRestoreFailed(state.ViewModelType, exception);
        }
    }

    [LoggerMessage(LogLevel.Error, "Navigation to {ViewModelType} failed.")]
    private partial void LogNavigationFailed(Type viewModelType, Exception exception);

    [LoggerMessage(LogLevel.Error, "Restoring {ViewModelType} after failed navigation failed.")]
    private partial void LogNavigationRestoreFailed(Type viewModelType, Exception exception);

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
