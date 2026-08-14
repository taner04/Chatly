using Chatly.Desktop.Abstractions.Overlays;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.DependencyInjection;

internal static class PopupOverlayServiceCollectionExtensions
{
    internal static IServiceCollection AddPopup<TOverlay, TViewModel>(this IServiceCollection services)
        where TOverlay : class, IPopupOverlay
        where TViewModel : class, IPopupViewModel
    {
        services.AddSingleton<IPopupOverlay, TOverlay>();
        services.AddTransient<TViewModel>();

        return services;
    }
}
