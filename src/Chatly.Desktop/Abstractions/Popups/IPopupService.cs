using System.Threading.Tasks;

namespace Chatly.Desktop.Abstractions.Popups;

public interface IPopupService
{
    void SetPopupHost(IPopupHost popupHost);

    Task ShowAsync<TViewModel>() where TViewModel : IPopupViewModel;
}