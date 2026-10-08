namespace Chatly.Desktop.Abstraction.Popups;

public interface IPopupService
{
    Task ShowAsync(Type popupViewModelType, CancellationToken cancellationToken = default);

    Task ShowAsync(IPopupViewModel popup, CancellationToken cancellationToken = default);
}