namespace Chatly.Desktop.Abstraction.Popups;

public interface IPopupService
{
    Task ShowAsync<TViewModel>() where TViewModel : IPopupViewModel;
}