using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.Services.Navigation;

[SingletonService(typeof(INavigationServiceFactory))]
public sealed class NavigationServiceFactory(IServiceProvider serviceProvider) : INavigationServiceFactory
{
    public INavigationService Create()
    {
        return ActivatorUtilities.CreateInstance<NavigationService>(serviceProvider);
    }
}