using System;
using Chatly.Desktop.Shared.Abstractions;

namespace Chatly.Desktop.Features.Popup;

public interface IMessageOverlayRegistration
{
    Type OverlayType { get; }

    MessageOverlayInstance Create();
}

public sealed record MessageOverlayInstance(IMessageOverlay Overlay, object View);
