using Chatly.Desktop.Services.Popups;

namespace Chatly.Desktop.Abstraction.Popups;

public interface IPopupHost
{
    event EventHandler<PopupOverlayEventArgs>? PopupEvent;

    void Show<TViewModel>(IPopupOverlay<TViewModel> overlay) where TViewModel : IPopupViewModel;

    void Close();
}