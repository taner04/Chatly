using Microsoft.Extensions.Logging;

namespace Chatly.Desktop.Services.Navigation;

public sealed partial class NavigationService
{
    private async Task NotifyNavigatedFromAsync(INavigableViewModel? viewModel)
    {
        try
        {
            switch (viewModel)
            {
                case INavigationParameterAware parameterAware:
                    await parameterAware.OnNavigatedFromAsync();
                    break;
                case INavigationAware navigationAware:
                    await navigationAware.OnNavigatedFromAsync();
                    break;
            }
        }
        catch (Exception exception)
        {
            LogNavigationFromFailed(viewModel?.GetType(), exception);
        }
    }

    private async Task NotifyNavigatedToAsync(INavigableViewModel viewModel, object? parameter)
    {
        try
        {
            if (parameter is not null && viewModel is INavigationParameterAware parameterAware)
            {
                await parameterAware.OnNavigatedToAsync(parameter);
            }
            else if (viewModel is INavigationAware navigationAware)
            {
                await navigationAware.OnNavigatedToAsync();
            }
        }
        catch (Exception exception)
        {
            LogNavigationToFailed(viewModel.GetType(), exception);
        }
    }

    [LoggerMessage(LogLevel.Error, "Navigation lifecycle failed while leaving {ViewModelType}.")]
    private partial void LogNavigationFromFailed(Type? viewModelType, Exception exception);

    [LoggerMessage(LogLevel.Error, "Navigation lifecycle failed while entering {ViewModelType}.")]
    private partial void LogNavigationToFailed(Type viewModelType, Exception exception);
}