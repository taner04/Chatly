using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chatly.Desktop.Abstractions.Overlays;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Chatly.Desktop.ViewModels.Overlays;

public sealed partial class PopupOverlayHostViewModel(IEnumerable<IPopupOverlay> popupOverlays) : ViewModelBase
{
    [ObservableProperty]
    public partial bool IsOpen { get; private set; }

    [ObservableProperty]
    public partial string? Title { get; private set; }

    [ObservableProperty]
    public partial object? Content { get; private set; }

    public async Task ShowAsync<TViewModel>(string title) where TViewModel : IPopupViewModel
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ThrowIfAlreadyOpen();

        var popupOverlay = popupOverlays.OfType<IPopupOverlay<TViewModel>>().FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"No popup overlay registered for view model {typeof(TViewModel).Name}.");

        Content = popupOverlay;
        Title = title;
        IsOpen = true;

        try
        {
            await popupOverlay.ViewModel.Completion;
        }
        finally
        {
            Title = null;
            IsOpen = false;
            Content = null;
        }
    }

    private void ThrowIfAlreadyOpen()
    {
        if (IsOpen)
        {
            throw new InvalidOperationException("A popup is already open.");
        }
    }
}
