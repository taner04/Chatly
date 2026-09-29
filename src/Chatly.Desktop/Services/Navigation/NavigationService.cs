using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.Services.Navigation;

[SingletonService(typeof(INavigationService))]
internal sealed partial class NavigationService(
    IServiceProvider serviceProvider,
    ILogger<NavigationService> logger) : INavigationService
{
    private readonly Stack<NavigationEntry> _backStack = new();
    private readonly Stack<NavigationEntry> _forwardStack = new();
    private readonly SemaphoreSlim _transitionLock = new(1, 1);
    private NavigationEntry? _current;

    public event EventHandler<NavigatedEventArgs>? Navigated;

    public INavigableViewModel? CurrentPage => _current?.Page;

    public Task<bool> NavigateToAsync(Type viewModelType, CancellationToken cancellationToken = default) =>
        NavigateCoreAsync(viewModelType, null, cancellationToken);

    public Task<bool> NavigateToAsync(
        Type viewModelType,
        object parameter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        return NavigateCoreAsync(viewModelType, parameter, cancellationToken);
    }

    public Task<bool> GoBackAsync(CancellationToken cancellationToken = default) =>
        NavigateHistoryAsync(_backStack, _forwardStack, cancellationToken);

    public Task<bool> GoForwardAsync(CancellationToken cancellationToken = default) =>
        NavigateHistoryAsync(_forwardStack, _backStack, cancellationToken);

    private async Task<bool> NavigateCoreAsync(
        Type viewModelType,
        object? parameter,
        CancellationToken cancellationToken)
    {
        ValidateViewModelType(viewModelType);

        NavigationEntry target;
        await _transitionLock.WaitAsync(cancellationToken);
        try
        {
            var previous = _current;
            var isSamePage = previous?.Page.GetType() == viewModelType;
            if (isSamePage && (parameter is null || Equals(previous?.Parameter, parameter)))
            {
                return false;
            }

            var page = (INavigableViewModel)serviceProvider.GetRequiredService(viewModelType);
            target = new NavigationEntry(page, parameter);
            if (!await TransitionAsync(previous, target, cancellationToken))
            {
                return false;
            }

            if (previous is not null && !isSamePage)
            {
                _backStack.Push(previous);
            }

            _forwardStack.Clear();
            _current = target;
        }
        finally
        {
            _transitionLock.Release();
        }

        Navigated?.Invoke(this, new NavigatedEventArgs(target.Page));
        return true;
    }

    private async Task<bool> NavigateHistoryAsync(
        Stack<NavigationEntry> source,
        Stack<NavigationEntry> destination,
        CancellationToken cancellationToken)
    {
        NavigationEntry? target;
        await _transitionLock.WaitAsync(cancellationToken);
        try
        {
            if (!source.TryPeek(out target))
            {
                return false;
            }

            var previous = _current;
            if (!await TransitionAsync(previous, target, cancellationToken))
            {
                return false;
            }

            source.Pop();
            if (previous is not null)
            {
                destination.Push(previous);
            }

            _current = target;
        }
        finally
        {
            _transitionLock.Release();
        }

        Navigated?.Invoke(this, new NavigatedEventArgs(target.Page));
        return true;
    }

    private async Task<bool> TransitionAsync(
        NavigationEntry? previous,
        NavigationEntry target,
        CancellationToken cancellationToken)
    {
        var targetEntered = false;

        try
        {
            if (previous is not null)
            {
                await previous.Page.OnNavigatedFromAsync(cancellationToken);
            }

            targetEntered = true;
            await target.Page.OnNavigatedToAsync(target.Parameter, cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await RollbackAsync(previous, target, targetEntered);
            throw;
        }
        catch (Exception exception)
        {
            LogNavigationFailed(target.Page.GetType(), exception);
            await RollbackAsync(previous, target, targetEntered);
            return false;
        }
    }

    private async Task RollbackAsync(NavigationEntry? previous, NavigationEntry target, bool targetEntered)
    {
        if (targetEntered)
        {
            try
            {
                await target.Page.OnNavigatedFromAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                LogNavigationCleanupFailed(target.Page.GetType(), exception);
            }
        }

        if (previous is null)
        {
            return;
        }

        try
        {
            await previous.Page.OnNavigatedToAsync(previous.Parameter, CancellationToken.None);
        }
        catch (Exception exception)
        {
            LogNavigationRestoreFailed(previous.Page.GetType(), exception);
        }
    }

    [LoggerMessage(LogLevel.Error, "Navigation to {ViewModelType} failed.")]
    private partial void LogNavigationFailed(Type viewModelType, Exception exception);

    [LoggerMessage(LogLevel.Error, "Leaving {ViewModelType} after failed navigation failed.")]
    private partial void LogNavigationCleanupFailed(Type viewModelType, Exception exception);

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

    private sealed record NavigationEntry(INavigableViewModel Page, object? Parameter);
}