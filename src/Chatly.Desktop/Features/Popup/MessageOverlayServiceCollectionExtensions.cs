using Avalonia.Controls;
using Chatly.Desktop.Shared.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.Features.Popup;

internal static class MessageOverlayServiceCollectionExtensions
{
    internal static IServiceCollection AddMessageOverlay<TOverlay, TView>(this IServiceCollection services)
        where TOverlay : class, IMessageOverlay
        where TView : Control
    {
        services.AddSingleton<IMessageOverlayRegistration, MessageOverlayRegistration<TOverlay, TView>>();
        return services;
    }
}
