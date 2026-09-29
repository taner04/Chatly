namespace Chatly.Desktop.ViewModels.Popups;

[SingletonService]
public sealed partial class PopupOverlayHostViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOpen))]
    public partial IPopupViewModel? Current { get; set; }

    public bool IsOpen => Current is not null;
}