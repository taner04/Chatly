using Chatly.Desktop.Services.Popups;
using System;

namespace Chatly.Desktop.Abstractions.Popups;

public interface IPopupHost
{
    event EventHandler<PopupOverlayEventArgs>? PopupEvent;

    void Show<TViewModel>(IPopupOverlay<TViewModel> overlay) where TViewModel : IPopupViewModel;

    void Close();
}