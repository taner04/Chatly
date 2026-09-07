namespace Chatly.Desktop.Abstraction.Popups;

public interface IPopupService
{
    void SetPopupHost(IPopupHost popupHost);

    Task ShowAsync<TViewModel>() where TViewModel : IPopupViewModel;
}