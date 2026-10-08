using Chatly.Desktop.ViewModels.Popups;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.Services.Popups;

[SingletonService(typeof(IPopupService))]
internal sealed class PopupService(
    IServiceProvider serviceProvider,
    PopupOverlayHostViewModel popupHost) : IPopupService
{
    public Task ShowAsync(Type popupViewModelType, CancellationToken cancellationToken = default)
    {
        popupViewModelType.ThrowIfNotAssignableTo<IPopupViewModel>();

        return popupHost.Current is null
            ? ShowAsync((IPopupViewModel)serviceProvider.GetRequiredService(popupViewModelType), cancellationToken)
            : Task.CompletedTask;
    }

    public async Task ShowAsync(IPopupViewModel popup, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(popup);

        if (popupHost.Current is not null)
        {
            return;
        }

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