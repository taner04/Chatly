namespace Chatly.Desktop.Abstractions.Popups;

public interface IPopupHost
{
    void Show<TViewModel>(IPopupOverlay<TViewModel> overlay) where TViewModel : IPopupViewModel;

    void Close();
}