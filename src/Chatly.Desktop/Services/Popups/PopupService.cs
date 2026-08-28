using System;
using System.Threading.Tasks;
using Chatly.Desktop.Abstractions.Popups;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.Services.Popups;

public sealed class PopupService(IServiceProvider serviceProvider) : IPopupService
{
    private IPopupHost? _popupHost;
    private bool _isOpen;
    
    public void SetPopupHost(IPopupHost popupHost)
    {
        _popupHost = popupHost ?? throw new ArgumentNullException(nameof(popupHost));
    }

    public async Task ShowAsync<TViewModel>() where TViewModel : IPopupViewModel
    {
        ThrowIfPopupIsOpen();

        var popupHost = GetPopupHost();
        var popupOverlay = serviceProvider.GetRequiredService<IPopupOverlay<TViewModel>>();

        popupHost.PopupEvent += popupOverlay.HandlePopupEvent;

        _isOpen = true;

        try
        {
            popupHost.Show(popupOverlay);
            await popupOverlay.ViewModel.Completion;
        }
        finally
        {
            try
            {
                popupHost.Close();
            }
            finally
            {
                _isOpen = false;
            }

            popupHost.PopupEvent -= popupOverlay.HandlePopupEvent;
        }
    }

    private IPopupHost GetPopupHost()
    {
        return _popupHost ?? throw new InvalidOperationException(
            "A popup host must be set before showing a popup.");
    }

    private void ThrowIfPopupIsOpen()
    {
        if (_isOpen)
        {
            throw new InvalidOperationException("A popup is already open.");
        }
    }
}
