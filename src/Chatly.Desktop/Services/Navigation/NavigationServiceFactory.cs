using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.Services.Navigation;

[SingletonService(typeof(INavigationServiceFactory))]
internal sealed class NavigationServiceFactory(IServiceProvider serviceProvider) : INavigationServiceFactory
{
    public INavigationService Create()
    {
        return ActivatorUtilities.CreateInstance<NavigationService>(serviceProvider);
    }
}
