using CommunityToolkit.Mvvm.ComponentModel;

namespace Chatly.Desktop.ViewModels.Popups;

public sealed partial class PopupOverlayHostViewModel : ViewModelBase
{
    [ObservableProperty] public partial bool IsOpen { get; set; }

    [ObservableProperty] public partial string? Title { get; set; }

    [ObservableProperty] public partial object? Content { get; set; }
}