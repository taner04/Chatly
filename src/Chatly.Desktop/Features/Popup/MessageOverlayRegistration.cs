using System;
using Avalonia.Controls;
using Chatly.Desktop.Shared.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.Features.Popup;

internal sealed class MessageOverlayRegistration<TOverlay, TView>(IServiceProvider serviceProvider)
    : IMessageOverlayRegistration
    where TOverlay : class, IMessageOverlay
    where TView : Control
{
    public Type OverlayType => typeof(TOverlay);

    public MessageOverlayInstance Create()
    {
        var overlay = ActivatorUtilities.CreateInstance<TOverlay>(serviceProvider);
        var view = ActivatorUtilities.CreateInstance<TView>(serviceProvider, overlay);
        return new MessageOverlayInstance(overlay, view);
    }
}
