using Chatly.Desktop.ViewModels.Popups;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.Services.Popups;

[SingletonService(typeof(IPopupService))]
internal sealed class PopupService(
    IServiceProvider serviceProvider,
    PopupOverlayHostViewModel popupHost) : IPopupService
{
    public async Task ShowAsync<TViewModel>() where TViewModel : IPopupViewModel
    {
        if (popupHost.Current is not null)
        {
            return;
        }

        var popup = serviceProvider.GetRequiredService<TViewModel>();
        popupHost.Current = popup;

        try
        {
            await popup.Completion;
        }
        finally
        {
            popupHost.Current = null;
        }
    }
}